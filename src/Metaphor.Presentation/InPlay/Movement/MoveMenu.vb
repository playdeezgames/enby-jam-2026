Imports Metaphor.Processing
Imports TGGD.Presentation

Friend Class MoveMenu
    Inherits MetaphorPickerMenu

    Private Sub New(context As IDisplayContext, model As IWorldModel, previous As DialogSource)
        MyBase.New(context, model, previous)
    End Sub

    Public Overrides ReadOnly Property PromptText As String
        Get
            Return "Move which way?"
        End Get
    End Property

    Protected Overrides ReadOnly Property Launchers As IEnumerable(Of LaunchDelegate)
        Get
            Return Enumerable.Empty(Of LaunchDelegate).
                Append(AddressOf ChooseNeverMind).
                Concat(Model.Exits.All.Select(AddressOf ChooseExit))
        End Get
    End Property

    Private Shared Function ChooseExit(exitModel As IExitModel) As LaunchDelegate
        Return Function(c, m, p) DialogChoice.CreateEnabled($"{exitModel.Name}({exitModel.Direction})", MoveActivity.Launch(c, m, p, exitModel))
    End Function

    Private Function ChooseNeverMind(context As IDisplayContext, model As IWorldModel, previous As DialogSource) As IDialogChoice
        Return DialogChoice.CreateEnabled("Never Mind", InPlay.Launch(context, model, previous))
    End Function

    Friend Shared Function Launch(context As IDisplayContext, model As IWorldModel, previous As DialogSource) As DialogSource
        Return Function() New MoveMenu(context, model, previous)
    End Function
End Class
