Imports Metaphor.Processing
Imports TGGD.Presentation

Friend Class ValidateChosenName
    Inherits KJDialog
    Const VALID_NAME = "Olen Kyrpa"
    Private ReadOnly chosenName As String

    Private Sub New(context As IDisplayContext, model As IWorldModel, previous As DialogSource, chosenName As String)
        MyBase.New(context, model, previous)
        Me.chosenName = chosenName
    End Sub

    Friend Shared Function Launch(context As IDisplayContext, model As IWorldModel, previous As DialogSource, chosenName As String) As DialogSource
        Return Function() New ValidateChosenName(context, model, previous, chosenName)
    End Function

    Public Overrides Function Run() As IDialogPrompt
        If chosenName.Equals(VALID_NAME, StringComparison.InvariantCultureIgnoreCase) Then
            Context.Render("Correct!")
        Else
            Context.Render($"Incorrect! Yer name is `{VALID_NAME}`! (Have you not played a SPLORR!! game before?)")
        End If
        Return DialogPrompt.CreateChoicePrompt("", DialogChoice.CreateEnabled("Next", AddressOf ChooseNext))
    End Function

    Private Function ChooseNext() As IDialog
        Return EmbarkActivity.Launch(Context, Model, Previous, VALID_NAME).Invoke()
    End Function
End Class
