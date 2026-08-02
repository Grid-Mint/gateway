# ---------- build ----------
FROM golang:1.26.5-bookworm AS builder

WORKDIR /app

# Спочатку тільки залежності — шар кешується, поки go.mod/go.sum не змінились
COPY src/go.* ./
RUN go mod download

COPY src/ ./

# CGO_ENABLED=0 — статичний бінарник, не залежить від glibc
RUN CGO_ENABLED=0 go build -v -ldflags="-s -w" -o server

# ---------- runtime ----------
FROM gcr.io/distroless/static-debian12

COPY --from=builder /app/server /app/server

EXPOSE 8080

USER nonroot:nonroot

ENTRYPOINT ["/app/server"]