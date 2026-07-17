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
        If Model.Avatar.IsDone Then
            Return DonePrompt.Launch(Context, Model, Previous).Invoke().Run()
        End If
        If Model.Avatar.Pace.IsChanging Then
            Return ChangePacePrompt.Launch(Context, Model, Previous).Invoke().Run
        End If
        Return NavigationMenu.Launch(Context, Model, Previous).Invoke().Run()
    End Function
End Class
