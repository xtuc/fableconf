module App.State

open Elmish
open Elmish.Browser.Navigation
open Elmish.Browser.UrlParser
open Fable.Import.Browser
open Global
open Types

let pageParser: Parser<Page->Page,Page> =
  oneOf [
    map Location (s "location")
    map (Some >> Speakers) (s "speakers" </> str)
    map (Speakers None) (s "speakers")
    map Home (s "home")
  ]

let urlUpdate (result: Option<Page>) model =
  match result with
  | None ->
    console.error("Error parsing url")
    model, Navigation.modifyUrl (toHash model.currentPage)
  | Some(Speakers speaker) ->
      let shuffledSpeakers = Speakers.State.shuffle model.speakers.speakers
      let modal =
        match speaker with
        | Some speaker ->
          let speaker = Speakers.Types.speakersMap |> Map.find speaker
          Some(speaker, defaultArg speaker.talk (Speakers.Types.defaultTalk()))
        | None -> model.speakers.modal
      let speakers =
        {model.speakers with speakers = shuffledSpeakers; modal = modal }
      { model with speakers = speakers; currentPage = Speakers speaker }, []
  | Some page ->
    { model with currentPage = page }, []

let init result =
  let (speakers, speakersCmd) = Speakers.State.init()
  let (navbar, maCmd) = Navbar.State.init()
  let (home, homeCmd) = Home.State.init()
  let (model, cmd) =
    urlUpdate result
      { currentPage = Home
        navbar = navbar
        speakers = speakers
        home = home }
  model, Cmd.batch [ cmd
                     Cmd.map SpeakersMsg speakersCmd
                     Cmd.map HomeMsg homeCmd ]

let update msg model =
  match msg with
  | NavbarMsg msg ->
      let (navbar, navbarCmd) = Navbar.State.update msg model.navbar
      { model with navbar = navbar }, Cmd.map NavbarMsg navbarCmd
  | SpeakersMsg msg ->
      let (speakers, speakersCmd) = Speakers.State.update msg model.speakers
      { model with speakers = speakers }, Cmd.map SpeakersMsg speakersCmd
  | HomeMsg msg ->
      let (home, homeCmd) = Home.State.update msg model.home
      { model with home = home }, Cmd.map HomeMsg homeCmd
