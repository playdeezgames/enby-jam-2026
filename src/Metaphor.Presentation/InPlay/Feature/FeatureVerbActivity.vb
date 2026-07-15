Imports Metaphor.Processing
Imports TGGD.Presentation

Friend Class FeatureVerbActivity
    Inherits MetaphorPickerMenu

    Private ReadOnly featureModel As IFeatureModel
    Private ReadOnly verbModel As IVerbModel

    Private Sub New(context As IDisplayContext, model As IWorldModel, previous As DialogSource, featureModel As IFeatureModel, verbModel As IVerbModel)
        MyBase.New(context, model, previous)
        Me.featureModel = featureModel
        Me.verbModel = verbModel
    End Sub

    Public Overrides ReadOnly Property PromptText As String
        Get
            Return String.Empty
        End Get
    End Property

    Protected Overrides ReadOnly Property Launchers As IEnumerable(Of LaunchDelegate)
        Get
            Return Enumerable.Empty(Of LaunchDelegate).
                Append(AddressOf ChooseOk)
        End Get
    End Property

    Private Function ChooseOk(context As IDisplayContext, model As IWorldModel, previous As DialogSource) As IDialogChoice
        Return DialogChoice.CreateEnabled("OK", FeatureMenu.Launch(context, model, Previous, featureModel))
    End Function

    Friend Shared Function Launch(c As IDisplayContext, m As IWorldModel, p As DialogSource, featureModel As IFeatureModel, verbModel As IVerbModel) As DialogSource
        Return Function()
                   verbModel.Perform()
                   Return New FeatureVerbActivity(c, m, p, featureModel, verbModel)
               End Function
    End Function
End Class
