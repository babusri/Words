namespace Words

open Xamarin.Forms

type App() =
    inherit Application()
    do
        base.MainPage <- new Words.MainPage()
