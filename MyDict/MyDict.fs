namespace MyDict

open Xamarin.Forms

type App() =
    inherit Application()
    do
        base.MainPage <- new MyDict.MainPage()
