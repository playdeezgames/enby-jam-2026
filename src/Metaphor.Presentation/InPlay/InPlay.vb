Imports Metaphor.Processing
Imports TGGD.Presentation

Friend Class InPlay
    Inherits MetaphorDialog

    Private Sub New(context As IDisplayContext, model As IWorldModel, previous As DialogSource)
        MyBase.New(context, model, previous)
    End Sub

    Friend Shared Function Launch(context As IDisplayContext, model As IWorldModel, previous As DialogSource) As DialogSource
        Return Function() New InPlay(context, model, previous)
    End Function

    Public Overrides Function Run() As IDialogPrompt
        If Model.IsInCombat Then
            Return CombatMenu.Launch(Context, Model, Previous).Invoke().Run()
        End If
        Return NavigationMenu.Launch(Context, Model, Previous).Invoke().Run()
    End Function
End Class
