Imports Metaphor.Processing
Imports TGGD.Presentation

Friend Class MoveActivity
    Inherits MetaphorDialog

    Private ReadOnly exitModel As IExitModel

    Private Sub New(context As IDisplayContext, model As IWorldModel, previous As DialogSource, exitModel As IExitModel)
        MyBase.New(context, model, previous)
        Me.exitModel = exitModel
    End Sub

    Friend Shared Function Launch(context As IDisplayContext, model As IWorldModel, previous As DialogSource, exitModel As IExitModel) As DialogSource
        Return Function() New MoveActivity(context, model, previous, exitModel)
    End Function

    Public Overrides Function Run() As IDialogPrompt
        exitModel.AttemptTake()
        Return InPlay.Launch(Context, Model, Previous).Invoke().Run()
    End Function
End Class
