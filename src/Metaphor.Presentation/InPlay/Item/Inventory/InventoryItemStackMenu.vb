Imports Metaphor.Processing
Imports TGGD.Presentation

Friend Class InventoryItemStackMenu
    Inherits MetaphorPickerMenu

    Private ReadOnly itemStackModel As IItemStackModel

    Private Sub New(context As IDisplayContext, model As IWorldModel, previous As DialogSource, itemStackModel As IItemStackModel)
        MyBase.New(context, model, previous)
        Me.itemStackModel = itemStackModel
    End Sub

    Public Overrides ReadOnly Property PromptText As String
        Get
            Return "Items in stack:"
        End Get
    End Property

    Protected Overrides ReadOnly Property Launchers As IEnumerable(Of LaunchDelegate)
        Get
            Return Enumerable.Empty(Of LaunchDelegate).
                Append(AddressOf ChooseNeverMind).
                Concat(itemStackModel.Items.Select(AddressOf ChooseItem))
        End Get
    End Property

    Private Function ChooseItem(itemModel As IItemModel) As LaunchDelegate
        Return Function(c, m, p) DialogChoice.CreateEnabled(itemModel.Name, InventoryItemMenu.Launch(c, m, p, itemModel))
    End Function

    Private Function ChooseNeverMind(context As IDisplayContext, model As IWorldModel, previous As DialogSource) As IDialogChoice
        Return DialogChoice.CreateEnabled(
            "Never Mind",
            InventoryMenu.Launch(context, model, previous))
    End Function

    Friend Shared Function Launch(c As IDisplayContext, m As IWorldModel, p As DialogSource, itemStackModel As IItemStackModel) As DialogSource
        Return Function()
                   Select Case itemStackModel.Count
                       Case 0
                           Return InventoryMenu.Launch(c, m, p).Invoke()
                       Case 1
                           Return InventoryItemMenu.Launch(c, m, p, itemStackModel.Top).Invoke()
                       Case Else
                           Return New InventoryItemStackMenu(c, m, p, itemStackModel)
                   End Select
               End Function
    End Function
End Class
