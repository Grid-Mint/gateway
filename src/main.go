package main

import (
	"log"
	"net/http"

	"lacarte/users/api"
	"github.com/go-chi/chi/v5"
)


func main() {
	r := chi.NewRouter()
	r.Get("/user/{id}", api.GetUser)
	r.Get("/users", api.ListUsers)

	log.Println("listening on :8080")
	var run = http.ListenAndServe(":8080", r)

	if run != nil {
		log.Fatal(run)
	}
}
