Imports Metaphor.Processing
Imports TGGD.Presentation

Friend Class NavigationMenu
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
                Append(AddressOf ChooseMove).
                Append(AddressOf ChooseStatus).
                Append(AddressOf ChooseGround).
                Append(AddressOf ChooseInventory).
                Append(AddressOf ChooseCharacters).
                Append(AddressOf ChooseFeatures).
                Append(AddressOf ChooseLook).
                Concat(Model.Location.Verbs.Select(AddressOf ChooseVerb)).
                Append(AddressOf ChooseGameMenu)
        End Get
    End Property

    Private Function ChooseStatus(context As IDisplayContext, model As IWorldModel, previous As DialogSource) As IDialogChoice
        Return DialogChoice.CreateEnabled("Status...", StatusActivity.LaunchStatusActivity(context, model, previous))
    End Function

    Private Function ChooseVerb(verbModel As IVerbModel) As LaunchDelegate
        Return Function(c, m, p) DialogChoice.Create(verbModel.IsEnabled, verbModel.Name, LocationVerbActivity.Launch(c, m, p, verbModel))
    End Function

    Private Function ChooseCharacters(context As IDisplayContext, model As IWorldModel, previous As DialogSource) As IDialogChoice
        Return DialogChoice.Create(model.Characters.HasAny, "Characters...", CharactersMenu.Launch(context, model, previous))
    End Function

    Private Function ChooseFeatures(context As IDisplayContext, model As IWorldModel, previous As DialogSource) As IDialogChoice
        Return DialogChoice.Create(model.Features.HasAny, "Features...", FeaturesMenu.Launch(context, model, previous))
    End Function

    Private Function ChooseInventory(
                                    context As IDisplayContext,
                                    model As IWorldModel,
                                    previous As DialogSource) As IDialogChoice
        Return DialogChoice.Create(model.Inventory.HasItems, "Inventory...", InventoryMenu.Launch(context, model, previous))
    End Function

    Private Function ChooseLook(context As IDisplayContext, model As IWorldModel, previous As DialogSource) As IDialogChoice
        Return DialogChoice.CreateEnabled("Look", LookActivity.Launch(context, model, previous))
    End Function

    Private Function ChooseGround(context As IDisplayContext, model As IWorldModel, previous As DialogSource) As IDialogChoice
        Return DialogChoice.Create(model.Ground.HasItems, "Ground...", GroundMenu.Launch(context, model, previous))
    End Function

    Private Function ChooseMove(context As IDisplayContext, model As IWorldModel, previous As DialogSource) As IDialogChoice
        Return DialogChoice.Create(model.Exits.HasAny, "Move...", MoveMenu.Launch(context, model, previous))
    End Function

    Private Function ChooseGameMenu(context As IDisplayContext, model As IWorldModel, previous As DialogSource) As IDialogChoice
        Return DialogChoice.CreateEnabled("Gämë Mënü", GameMenu.Launch(context, model, previous))
    End Function

    Friend Shared Function Launch(context As IDisplayContext, model As IWorldModel, previous As DialogSource) As DialogSource
        Return Function() New NavigationMenu(context, model, previous)
    End Function
End Class
