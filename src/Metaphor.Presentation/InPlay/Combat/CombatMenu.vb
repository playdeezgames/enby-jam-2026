
Imports Metaphor.Processing
Imports TGGD.Presentation

Friend Class CombatMenu
    Inherits KJPickerMenu

    Private Sub New(context As IDisplayContext, model As IWorldModel, previous As DialogSource)
        MyBase.New(context, model, previous)
    End Sub

    Public Overrides ReadOnly Property PromptText As String
        Get
            Return "Now What?"
        End Get
    End Property

    Protected Overrides ReadOnly Property Launchers As IEnumerable(Of LaunchDelegate)
        Get
            Return Enumerable.Empty(Of LaunchDelegate).
                Concat(Model.Location.Others.Select(AddressOf ChooseAttack)).
                Append(AddressOf ChooseRun)
        End Get
    End Property

    Private Function ChooseAttack(characterModel As ICharacterModel) As LaunchDelegate
        Return Function(c, m, p)
                   Return DialogChoice.CreateEnabled($"Attack {characterModel.Name}", AttackActivity.LaunchAttackActivity(c, m, p, characterModel))
               End Function
    End Function

    Private Function ChooseRun(context As IDisplayContext, model As IWorldModel, previous As DialogSource) As IDialogChoice
        Return DialogChoice.Create(True, "Run!", RunActivity.LaunchRunActivity(context, model, previous))
    End Function

    Friend Shared Function Launch(context As IDisplayContext, model As IWorldModel, previous As DialogSource) As DialogSource
        Return Function() New CombatMenu(context, model, previous)
    End Function

End Class
