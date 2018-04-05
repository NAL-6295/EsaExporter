package main

import (
	"encoding/json"
	"flag"
	"fmt"
	"net/http"
	"os"
	"strconv"
)

var endpoint = "https://api.esa.io/v1"
var token = flag.String("token", "token", "access token")
var fromteam = flag.String("teamname", "teamname", "teamname ***.esa.io")
var rootpath = flag.String("filepath", "D:\\", "filepathroot")
var basepath = ".esa.io"

func Exists(filename string) bool {
	_, err := os.Stat(filename)
	return err == nil
}

func main() {
	flag.Parse()
	fmt.Println("test")

	filepath := *rootpath + *fromteam + basepath
	if !Exists(filepath) {
		err := os.Mkdir(filepath, 0644)
		if err != nil {
			panic(err)
		}
	}

	page := 1
	nextPage := 0

	for {

		nextPage = requestPage(page)
		if nextPage == 0 {
			return
		}

		page++
	}

}

func requestPage(page int) int {
	authorizationValue := "Bearer " + *token
	var url = endpoint + "/teams/" + *fromteam + "/posts?page=" + strconv.Itoa(page)
	req, err := http.NewRequest("GET", url, nil)
	if err != nil {
		panic(err)
	}

	req.Header.Add("Authorization", authorizationValue)

	client := &http.Client{}

	resp, err := client.Do(req)

	if err != nil {
		panic(err)
	}
	defer resp.Body.Close()

	var posts Posts
	json.NewDecoder(resp.Body).Decode(&posts)

	for _, post := range posts.Posts {
		ToLocal(post)
	}
	fmt.Println("page:" + strconv.Itoa(page))

	return posts.NextPage
}

func ToLocal(post Post) {
	fmt.Println(post)
}
