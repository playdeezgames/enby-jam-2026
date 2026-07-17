
Imports Metaphor.Processing
Imports TGGD.Presentation

Friend Class ChangePacePrompt
    Inherits MetaphorPickerMenu

    Friend Sub New(context As IDisplayContext, model As IWorldModel, previous As DialogSource)
        MyBase.New(context, model, previous)
    End Sub

    Public Overrides ReadOnly Property PromptText As String
        Get
            Return "Choose Pace:"
        End Get
    End Property

    Protected Overrides ReadOnly Property Launchers As IEnumerable(Of LaunchDelegate)
        Get
            Return Enumerable.Empty(Of LaunchDelegate).
                Append(AddressOf ChooseNeverMind).
                Concat(Enumerable.Range(1, 5).Select(AddressOf ChoosePace))
        End Get
    End Property

    Private ReadOnly paceName As New Dictionary(Of Integer, String) From
        {
            {1, "Very Slow"},
            {2, "Slow"},
            {3, "Moderate"},
            {4, "Fast"},
            {5, "Very Fast"}
        }

    Private Function ChoosePace(pace As Integer) As LaunchDelegate
        Return Function(c, m, p)
                   Return DialogChoice.CreateEnabled($"{paceName(pace)}({pace})", SetPaceActivity.Launch(c, m, p, pace))
               End Function
    End Function

    Private Function ChooseNeverMind(context As IDisplayContext, model As IWorldModel, previous As DialogSource) As IDialogChoice
        Return DialogChoice.CreateEnabled("Never Mind", CancelChangePaceActivity.Launch(context, model, previous))
    End Function

    Friend Shared Function Launch(context As IDisplayContext, model As IWorldModel, previous As DialogSource) As DialogSource
        Return Function() New ChangePacePrompt(context, model, previous)
    End Function
End Class
