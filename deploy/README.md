# Deploying to AKS

`manifest.yaml` in this folder deploys the Planner Ex-/Import app to an AKS
cluster with an nginx ingress controller.

## What gets deployed

All resources go into the `planner-exandimport` namespace.

| Component          | Kind                       | Exposure                                                        |
| ------------------ | -------------------------- | --------------------------------------------------------------- |
| `planner-backend`  | Deployment + Service       | Internal only (`ClusterIP`). Reached by the frontend in-cluster.|
| `planner-frontend` | Deployment + Service + Ingress | Public, under `/planner` on the shared ingress host.        |

- The **backend** just forwards the user's bearer token to
  `https://graph.microsoft.com`, so it needs **no** Azure AD configuration.
- The **frontend** is a Blazor Server app. It authenticates the user against
  Azure AD and calls the backend at `http://planner-backend` (cluster DNS).
- The frontend is served under the sub-path **`/planner`**. This relies on the
  `PathBase` env var (see the manifest) plus `UsePathBase`/`UseForwardedHeaders`
  support in `frontend/Program.cs`, so the app, its Blazor SignalR hub, static
  assets and the OIDC redirect all work behind the ingress under that path.

## Prerequisites

- `kubectl` pointed at the target AKS cluster.
- An nginx ingress controller and a TLS certificate already configured for the
  host `fps-alpaca.westeurope.cloudapp.azure.com` (the ingress reuses it; no
  `secretName` is set, matching the existing convention on this cluster).
- The container images pushed to the registry:
  - `tobiasfenster/planner-exandimport-wasm-backend:latest`
  - `tobiasfenster/planner-exandimport-wasm-frontend:latest`

  These are built by the tag-triggered GitHub Actions workflows
  (`.github/workflows/build-*-image-on-tag.yml`). Push a `v*` tag to build and
  push images that include the current code (the `PathBase` support is required
  for the sub-path deployment):

  ```bash
  git tag v1.2.0 && git push origin v1.2.0
  ```

- A `regcred` image-pull secret in the namespace if the images are private.
  (They are public on Docker Hub, so this is optional — the reference to it is
  harmless if the secret does not exist.)

## 1. Register the Azure AD redirect URI

On the app registration (client id `dd0f61d4-5801-402d-8d75-a8e74af8d681`), add
this **Web** redirect URI:

```
https://fps-alpaca.westeurope.cloudapp.azure.com/planner/authentication/login-callback
```

This is the host + `/planner` path base + the app's configured `CallbackPath`.

## 2. Provide the client secret

The `AZUREAD__CLIENTSECRET` value in `manifest.yaml` is a placeholder
(`REPLACE_WITH_CLIENT_SECRET`). Do **not** commit the real secret. Either edit
it locally right before applying, or set it separately after applying the
manifest:

```bash
kubectl -n planner-exandimport create secret generic planner-exandimport-secrets \
  --from-literal=AZUREAD__INSTANCE='https://login.microsoftonline.com/' \
  --from-literal=AZUREAD__TENANTID='92f4dd01-f0ea-4b5f-97f2-505c2945189c' \
  --from-literal=AZUREAD__CLIENTID='dd0f61d4-5801-402d-8d75-a8e74af8d681' \
  --from-literal=AZUREAD__CLIENTSECRET='<the-real-secret>' \
  --from-literal=AZUREAD__CALLBACKPATH='/authentication/login-callback' \
  --from-literal=AZUREAD__AUDIENCE='https://graph.microsoft.com/' \
  --dry-run=client -o yaml | kubectl apply -f -
```

## 3. Apply the manifest

```bash
kubectl apply -f deploy/manifest.yaml
```

## 4. Verify

```bash
kubectl -n planner-exandimport get pods,svc,ingress
kubectl -n planner-exandimport rollout status deployment/planner-backend
kubectl -n planner-exandimport rollout status deployment/planner-frontend
```

Then browse to:

```
https://fps-alpaca.westeurope.cloudapp.azure.com/planner
```

You should be redirected to the Microsoft sign-in page and, after logging in,
land back in the app.

## Configuration reference

Frontend environment (set via the Secret and the Deployment's `env`):

| Variable                | Value / source                          | Purpose                                    |
| ----------------------- | --------------------------------------- | ------------------------------------------ |
| `PathBase`              | `/planner`                              | Sub-path the app is served under.          |
| `BackendBaseUrl`        | `http://planner-backend`                | In-cluster address of the backend service. |
| `ASPNETCORE_URLS`       | `http://+:8080`                         | Kestrel listens on 8080 (non-privileged).  |
| `ASPNETCORE_ENVIRONMENT`| `Production`                            | Environment.                               |
| `AZUREAD__*`            | Secret `planner-exandimport-secrets`    | Azure AD / OpenID Connect settings.        |

To change the host or path, edit the `Ingress` rule and the frontend
`PathBase` env together (they must match), and update the registered Azure AD
redirect URI accordingly.

## Troubleshooting

- **Login redirects to `http://…` or a URL without `/planner`** — the ingress
  is not sending `X-Forwarded-Proto`, or `PathBase` is not set. Confirm the
  frontend `PathBase` env and that `UseForwardedHeaders`/`UsePathBase` run before
  authentication (they do in `frontend/Program.cs`).
- **`AADSTS50011: redirect URI mismatch`** — the URI in step 1 was not
  registered exactly, including the `/planner` segment and `https`.
- **Frontend cannot reach the backend** — check `BackendBaseUrl` matches the
  backend Service name/namespace and that both pods are `Running`.
