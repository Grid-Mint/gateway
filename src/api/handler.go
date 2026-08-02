package api

import (
	"context"
	"io"
	"log"
	"net/http"
	"os"
	"time"
)

var (
	httpClient = &http.Client{Timeout: 5 * time.Second}
	usersAPI   = os.Getenv("USERS__API")
)

func GetUser(w http.ResponseWriter, r *http.Request) {
	id := r.PathValue("id")
	log.Printf("%s %s id=%s", r.Method, r.URL.Path, id)

	ctx, cancel := context.WithTimeout(r.Context(), 5*time.Second)
	defer cancel()

	req, err := http.NewRequestWithContext(ctx, http.MethodGet,
		usersAPI+"/users/"+id, nil)
	if err != nil {
		http.Error(w, "internal error", http.StatusInternalServerError)
		return
	}

	resp, err := httpClient.Do(req)
	if err != nil {
		http.Error(w, "upstream unavailable", http.StatusBadGateway)
		return
	}
	defer resp.Body.Close()

	body, err := io.ReadAll(resp.Body)
	if err != nil {
		http.Error(w, "internal error", http.StatusInternalServerError)
		return
	}

	w.WriteHeader(resp.StatusCode)
	w.Write([]byte("gateway: " + string(body)))
}

func ListUsers(w http.ResponseWriter, r *http.Request) {
	log.Printf("%s %s", r.Method, r.URL.Path)
	w.WriteHeader(http.StatusOK)
	w.Write([]byte("users"))
}
