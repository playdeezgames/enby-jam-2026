Imports Metaphor.Processing
Imports TGGD.Presentation

Public Class Title
    Inherits MetaphorDialog
    Implements IDialog

    Private Sub New(context As IDisplayContext, model As IWorldModel, previous As DialogSource)
        MyBase.New(context, model, previous)
    End Sub

    Public Overrides Function Run() As IDialogPrompt
        Context.Render("Gwen's Peregrination II", New Dictionary(Of String, String) From {{HintNames.ELEMENT_TYPE, ElementTypes.TITLE}})
        Context.Render("A Production of ", newLine:=False)
        Context.Render("TheGrumpyGameDev", New Dictionary(Of String, String) From {{HintNames.ELEMENT_TYPE, ElementTypes.LINK}, {HintNames.URL, "https://thegrumpygamedev.itch.io/"}})
        Context.Render("Made for ", newLine:=False)
        Context.Render("Nonbinary Game Jam 2026", New Dictionary(Of String, String) From {{HintNames.ELEMENT_TYPE, ElementTypes.LINK}, {HintNames.URL, "https://itch.io/jam/nonbinary-game-jam-2026"}})
        Context.Render("Sponsored by: ", newLine:=False)
        Context.Render("UMLAUT.FYI!", New Dictionary(Of String, String) From {{HintNames.ELEMENT_TYPE, ElementTypes.LINK}, {HintNames.URL, "https://umlaut.fyi/"}})
        Return DialogPrompt.CreateChoicePrompt(
            "",
            DialogChoice.Create(True, "OK", MainMenu.Launch(Context, Model, Previous)))
    End Function

    Public Shared Function Launch(context As IDisplayContext, model As IWorldModel, previous As DialogSource) As DialogSource
        Return Function() New Title(context, model, previous)
    End Function

End Class
