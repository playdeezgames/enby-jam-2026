Imports Metaphor.Processing
Imports TGGD.Presentation

Friend Class TakeItemActivity
    Inherits KJDialog

    Private ReadOnly itemModel As IItemModel

    Private Sub New(context As IDisplayContext, model As IWorldModel, previous As DialogSource, itemModel As IItemModel)
        MyBase.New(context, model, previous)
        Me.itemModel = itemModel
    End Sub

    Friend Shared Function Launch(context As IDisplayContext, model As IWorldModel, previous As DialogSource, itemModel As IItemModel) As DialogSource
        Return Function() New TakeItemActivity(context, model, previous, itemModel)
    End Function

    Public Overrides Function Run() As IDialogPrompt
        itemModel.Take()
        Return GroundMenu.Launch(Context, Model, Previous).Invoke().Run()
    End Function


End Class
