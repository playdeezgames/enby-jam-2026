
Imports Metaphor.Processing
Imports TGGD.Presentation

Friend Class FeaturesMenu
    Inherits MetaphorPickerMenu

    Private Sub New(context As IDisplayContext, model As IWorldModel, previous As DialogSource)
        MyBase.New(context, model, previous)
    End Sub

    Public Overrides ReadOnly Property PromptText As String
        Get
            Return "Which feature?"
        End Get
    End Property

    Protected Overrides ReadOnly Property Launchers As IEnumerable(Of LaunchDelegate)
        Get
            Return Enumerable.Empty(Of LaunchDelegate).
                Append(AddressOf ChooseNeverMind).
                Concat(Model.Features.All.Select(AddressOf ChooseFeature))
        End Get
    End Property

    Private Function ChooseFeature(featureModel As IFeatureModel) As LaunchDelegate
        Return Function(c, m, p)
                   Return DialogChoice.CreateEnabled(featureModel.Name, FeatureMenu.Launch(c, m, p, featureModel))
               End Function
    End Function

    Private Function ChooseNeverMind(context As IDisplayContext, model As IWorldModel, previous As DialogSource) As IDialogChoice
        Return DialogChoice.CreateEnabled("Never Mind", InPlay.Launch(context, model, previous))
    End Function

    Friend Shared Function Launch(context As IDisplayContext, model As IWorldModel, previous As DialogSource) As DialogSource
        Return Function()
                   If model.Features.HasAny Then
                       model.Features.ShowList()
                       Return New FeaturesMenu(context, model, previous)
                   End If
                   Return InPlay.Launch(context, model, previous).Invoke()
               End Function
    End Function
End Class
