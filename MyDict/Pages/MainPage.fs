namespace Words

open System
open Xamarin.Forms
open System.Reactive.Linq
open System.Reactive.Concurrency

type MainPage() =
    inherit ContentPage()
    let rnd = System.Random()
    let orange = "FF6A00"
    let oneNumberEverySecond = Observable.Interval(TimeSpan.FromSeconds(1.))
    let mutable index = -1
    let mutable d1:IDisposable = null
    let mutable d2:IDisposable = null
    // TODO: replace with sorted list
    let wordsAndMeanings = Words.WordsList.wordsAndMeaningsSorted

    let numWords = Array.length(wordsAndMeanings)
    let showNextWordButton = new Button(Text = "Show next word")
    let showRandomWordButton = new Button(Text = "Show random word")

    let wordTextEntry = new Entry(
                                  Placeholder = "Touch any SHOW button",
                                  TextColor = Color.Yellow,
                                  BackgroundColor = Color.Black
                                  // Editable = false
                                  )

    let timeSlider = new Slider(Maximum = 4., Minimum = 0., Value = 2.,
                                HorizontalOptions = LayoutOptions.Fill)

    let delayInfoString(delay) = if (delay > 0.) then
                                     "Show meaning in " + string(delay) + " seconds"
                                 else
                                      ""

    // Change name as this is not a count down label
    let countDownLabel = new Label(Text = delayInfoString timeSlider.Value,
                                   XAlign = TextAlignment.Center,
                                   TextColor = Color.Olive,
                                   BackgroundColor = Color.White,
                                   FontAttributes = FontAttributes.Bold,
                                   IsEnabled=false
                                   )

    let wordsAndMeaningsTextEditor = new Editor(TextColor = Color.FromHex(orange),
                                                BackgroundColor = Color.Black,
                                                FontAttributes = FontAttributes.Bold,
                                                HorizontalOptions = LayoutOptions.Fill,
                                                VerticalOptions = LayoutOptions.FillAndExpand
                                                // Editable = false
                                                )

    let startingLetterLabel = new Label(Text = "A",
                                        YAlign = TextAlignment.Center,
                                        TextColor = Color.Yellow,
                                        FontAttributes = FontAttributes.Bold
                                        )
    let locationSlider = new Slider(Maximum = float(numWords-1), Minimum = 0., Value = 0.,
                                    HorizontalOptions = LayoutOptions.FillAndExpand)

    let dispAfterDelay (sec:float, str:string, target:Editor) =
        Observable.Timer(TimeSpan.FromSeconds(sec)).Subscribe(fun _ ->
                                                              Device.BeginInvokeOnMainThread(fun _ -> target.Text <- str))
    do
        let layout = StackLayout()
        layout.Children.Add(Label(Text = "Test your Vocabulary, v1.2\nAuthor: Babu Srinivasan\nhttp://blog.srinivasan.biz/android-apps\nNumber of words: " + string(numWords), TextColor = Color.Yellow))

        let layout2 = StackLayout(Orientation = StackOrientation.Horizontal)

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
                                                 locationSlider.IsEnabled <- true
                                                 startingLetterLabel.IsEnabled <- true
                                                 countDownLabel.Text <- delayInfoString timeSlider.Value)

        let wordButtonClicked() =

            showNextWordButton.IsEnabled <- false
            showRandomWordButton.IsEnabled <- false

            if (d1 <> null) then
                d1.Dispose()
                d1 <- null
            if (d2 <> null) then
                d2.Dispose()
                d2 <- null

            if (timeSlider.Value > 0.) then
                timeSlider.IsEnabled <- false
                locationSlider.IsEnabled <- false
                // $$ TODO startingLetterLabel.Text <- ""
                startingLetterLabel.IsEnabled <- false
                countDownLabel.Text <- string(timeSlider.Value)
            let wrd = wordsAndMeanings.[index]
            wordTextEntry.Text <- fst wrd
            wordsAndMeaningsTextEditor.Text <- ""
            if (timeSlider.Value > 0.) then
                d1 <- dispAfterDelay(timeSlider.Value, (snd wrd), wordsAndMeaningsTextEditor)
                // d2 <- oneNumberEverySecond.Subscribe(oneSecTimerSubFun)
                d2 <- oneNumberEverySecond.Subscribe(fun x -> oneSecTimerSubFun(int(x)))
            else
                wordsAndMeaningsTextEditor.Text <- snd wrd
            locationSlider.Value <- float(index)
            showNextWordButton.IsEnabled <- true
            showRandomWordButton.IsEnabled <- true

        showNextWordButton.Clicked.Add (fun eventargs ->
                                        index <- (index + 1) % numWords
                                        wordButtonClicked())


        showRandomWordButton.Clicked.Add (fun eventargs ->
                                          index <-  rnd.Next(numWords)
                                          wordButtonClicked())

        timeSlider.ValueChanged.Add(fun e ->
                                    let newval = Math.Round(e.NewValue)
                                    timeSlider.Value <- newval
                                    countDownLabel.Text <- delayInfoString timeSlider.Value
                                    )

        locationSlider.ValueChanged.Add(fun e ->
                                            // showNextWordButton.Text <- string(index) + ", " + string(e.NewValue) + ", " + string(index = int(e.NewValue))
                                            if (not ( ((float)index > e.NewValue) &&
                                                 ( (float(index) - e.NewValue) < 1.0 ))) then
                                                index <- int(e.NewValue)
                                            startingLetterLabel.Text <- (fst wordsAndMeanings.[index]).[0..0])

        List.map layout2.Children.Add ([startingLetterLabel
                                        locationSlider
                                       ]: View list) |> ignore

        List.map layout.Children.Add ([showNextWordButton
                                       showRandomWordButton
                                       timeSlider
                                       countDownLabel
                                       wordTextEntry
                                       wordsAndMeaningsTextEditor
                                       layout2
                                       ]: View list) |> ignore

        base.Content <- layout
