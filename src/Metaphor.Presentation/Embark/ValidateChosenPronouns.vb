Imports Metaphor.Processing
Imports TGGD.Presentation

Friend Class ValidateChosenPronouns
    Inherits MetaphorDialog
    Private ReadOnly chosenName As String
    Private ReadOnly chosenPronouns As String

    Private Sub New(context As IDisplayContext, model As IWorldModel, previous As DialogSource, chosenName As String, chosenPronouns As String)
        MyBase.New(context, model, previous)
        Me.chosenName = chosenName
        Me.chosenPronouns = chosenPronouns
    End Sub

    Friend Shared Function Launch(context As IDisplayContext, model As IWorldModel, previous As DialogSource, chosenName As String, chosenPronouns As String) As DialogSource
        Return Function() New ValidateChosenPronouns(context, model, previous, chosenName, chosenPronouns)
    End Function

    Public Overrides Function Run() As IDialogPrompt
        If chosenPronouns.Equals(ChoosePronounsPrompt.VALID_PRONOUNS, StringComparison.InvariantCultureIgnoreCase) Then
            Context.Render("Correct!")
        Else
            Context.Render($"Incorrect! {chosenName}'s pronouns are `{ChoosePronounsPrompt.VALID_PRONOUNS}`! (This metaphor is an entry in `Nonbinary Game Jam 2026`, so the main character has to be an enby. Next time, you can engage with the metaphor using whichever pronouns you feel most comfortable.)")
        End If
        Return DialogPrompt.CreateChoicePrompt("", DialogChoice.CreateEnabled("Next", AddressOf ChooseNext))
    End Function

    Private Function ChooseNext() As IDialog
        Return EmbarkActivity.Launch(Context, Model, Previous, chosenName, ChoosePronounsPrompt.VALID_PRONOUNS).Invoke()
    End Function

End Class
