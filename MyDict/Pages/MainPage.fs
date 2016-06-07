namespace MyDict

open System
open Xamarin.Forms
open System.Reactive.Linq
open System.Reactive.Concurrency

type MainPage() =
    inherit ContentPage()
    let rnd = System.Random()
    let oneNumberEverySecond = Observable.Interval(TimeSpan.FromSeconds(1.))
    let mutable index = 0
    let mutable d1:IDisposable = null
    let mutable d2:IDisposable = null
    let wordsAndMeanings = MyDict.Words.wordsAndMeanings

    let dispAfterDelay (sec:float, str:string, target:Editor) =
        Observable.Timer(TimeSpan.FromSeconds(sec)).Subscribe(fun _ ->
                                                              Device.BeginInvokeOnMainThread(fun _ -> target.Text <- str))

    let numWords = Array.length(wordsAndMeanings)
    let showNextWordButton = new Button(Text = "Show next word")
    let showRandomWordButton = new Button(Text = "Show random word")

    let wordTextEntry = new Entry(
                                  TextColor = Color.Yellow,
                                  BackgroundColor = Color.Black
                                  // Editable = false
                                  )

    let timeSlider = new Slider(Maximum = 4., Minimum = 0., Value = 2.)

    let delayInfoString(delay) = if (delay > 0.) then
                                     "Show meaning in " + string(delay) + " seconds"
                                 else
                                     "Show meaning right away"

    // Change name as this is not a count down label
    let countDownLabel = new Label(Text = delayInfoString timeSlider.Value,
                                   TextColor = Color.White,
                                   BackgroundColor = Color.Black,
                                   FontAttributes = FontAttributes.Bold,
                                   IsEnabled=false
                                   )

    // let fontSize = Device.GetNamedSize(NamedSize.Large:NamedSize, typeof<Label>:Element)

    let wordsAndMeaningsTextEditor = new Editor(TextColor = Color.Yellow,
                                                BackgroundColor = Color.Black,
                                                FontAttributes = FontAttributes.Bold,
                                                HorizontalOptions = LayoutOptions.Fill,
                                                VerticalOptions = LayoutOptions.FillAndExpand
                                                // Editable = false
                                                )

    let resetButton = new Button(Text = "Reset Sequence")

    do
        let layout = StackLayout()
        layout.Children.Add(Label(Text = "Vocabulary test: # of words " + string(numWords),
                                  TextColor = Color.Yellow))

        let oneSecTimerSubFun x =
            Device.BeginInvokeOnMainThread(fun _ ->
                                               let y = x + 1
                                               if (y < int(timeSlider.Value)) then
                                                  countDownLabel.Text <- string(int(timeSlider.Value) - y)
                                               else
                                                 if (d2 <> null) then
                                                    d2.Dispose()
                                                    d2 <- null
                                                 timeSlider.IsEnabled <- true
                                                 countDownLabel.Text <- delayInfoString timeSlider.Value)

        let wordButtonClicked() =
            if (timeSlider.Value > 0.) then
                timeSlider.IsEnabled <- false
                countDownLabel.Text <- string(timeSlider.Value)
            let wrd = wordsAndMeanings.[index]
            wordTextEntry.Text <- fst wrd
            wordsAndMeaningsTextEditor.Text <- ""
            if (d1 <> null) then
                d1.Dispose()
                d1 <- null
            if (d2 <> null) then
                d2.Dispose()
                d2 <- null
            if (timeSlider.Value > 0.) then
                d1 <- dispAfterDelay(timeSlider.Value, (snd wrd), wordsAndMeaningsTextEditor)
                // d2 <- oneNumberEverySecond.Subscribe(oneSecTimerSubFun)
                d2 <- oneNumberEverySecond.Subscribe(fun x -> oneSecTimerSubFun(int(x)))
            else
                wordsAndMeaningsTextEditor.Text <- snd wrd

        showNextWordButton.Clicked.Add (fun eventargs ->
                                          wordButtonClicked()
                                          index <- (index + 1) % numWords)

        showRandomWordButton.Clicked.Add (fun eventargs ->
                                          index <- rnd.Next(numWords)
                                          wordButtonClicked()
                                          index <- (index + 1) % numWords)

        timeSlider.ValueChanged.Add(fun e ->
                                    let newval = Math.Round(e.NewValue)
                                    timeSlider.Value <- newval
                                    countDownLabel.Text <- delayInfoString timeSlider.Value
                                    )

        resetButton.Clicked.Add (fun _ -> index <- 0)

        List.map layout.Children.Add ([showNextWordButton
                                       showRandomWordButton
                                       timeSlider
                                       countDownLabel
                                       wordTextEntry
                                       wordsAndMeaningsTextEditor
                                       resetButton
                                       ]: View list) |> ignore

        base.Content <- layout
