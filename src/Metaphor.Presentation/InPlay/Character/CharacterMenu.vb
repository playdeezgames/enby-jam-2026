
Imports Metaphor.Processing
Imports TGGD.Presentation

Friend Class CharacterMenu
    Inherits KJPickerMenu

    Private ReadOnly characterModel As ICharacterModel

    Public Sub New(context As IDisplayContext, model As IWorldModel, previous As DialogSource, characterModel As ICharacterModel)
        MyBase.New(context, model, previous)
        Me.characterModel = characterModel
    End Sub

    Public Overrides ReadOnly Property PromptText As String
        Get
            Return $"Do what with {characterModel.Name}?"
        End Get
    End Property

    Protected Overrides ReadOnly Property Launchers As IEnumerable(Of LaunchDelegate)
        Get
            Return Enumerable.Empty(Of LaunchDelegate).
                Append(AddressOf ChooseNeverMind).
                Concat(characterModel.Verbs.Select(AddressOf ChooseCharacterVerb))
        End Get
    End Property

    Private Function ChooseCharacterVerb(verbModel As IVerbModel) As LaunchDelegate
        Return Function(c, m, p)
                   Return DialogChoice.Create(verbModel.IsEnabled, verbModel.Name, CharacterVerbActivity.Launch(c, m, p, characterModel, verbModel))
               End Function
    End Function

    Friend Shared Function Launch(c As IDisplayContext, m As IWorldModel, p As DialogSource, characterModel As ICharacterModel) As DialogSource
        Return Function()
                   If m.IsInCombat Then
                       Return InPlay.Launch(c, m, p).Invoke()
                   End If
                   characterModel.Examine()
                   Return New CharacterMenu(c, m, p, characterModel)
               End Function
    End Function

    Private Function ChooseNeverMind(context As IDisplayContext, model As IWorldModel, previous As DialogSource) As IDialogChoice
        Return DialogChoice.CreateEnabled("Never Mind", CharactersMenu.Launch(context, model, previous))
    End Function
End Class
