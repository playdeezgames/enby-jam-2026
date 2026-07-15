
Imports Metaphor.Processing
Imports TGGD.Presentation

Friend Class CharactersMenu
    Inherits KJPickerMenu

    Private Sub New(context As IDisplayContext, model As IWorldModel, previous As DialogSource)
        MyBase.New(context, model, previous)
    End Sub

    Public Overrides ReadOnly Property PromptText As String
        Get
            Return "Which Character?"
        End Get
    End Property

    Protected Overrides ReadOnly Property Launchers As IEnumerable(Of LaunchDelegate)
        Get
            Return Enumerable.Empty(Of LaunchDelegate).
                Append(AddressOf ChooseNeverMind).
                Concat(Model.Characters.All.Select(AddressOf ChooseCharacter))
        End Get
    End Property

    Private Function ChooseCharacter(characterModel As ICharacterModel) As LaunchDelegate
        Return Function(c, m, p) DialogChoice.CreateEnabled(characterModel.Name, CharacterMenu.Launch(c, m, p, characterModel))
    End Function

    Friend Shared Function Launch(context As IDisplayContext, model As IWorldModel, previous As DialogSource) As DialogSource
        Return Function()
                   model.Characters.ShowList()
                   Return New CharactersMenu(context, model, previous)
               End Function
    End Function

    Private Function ChooseNeverMind(context As IDisplayContext, model As IWorldModel, previous As DialogSource) As IDialogChoice
        Return DialogChoice.CreateEnabled("Never Mind", InPlay.Launch(context, model, previous))
    End Function
End Class
