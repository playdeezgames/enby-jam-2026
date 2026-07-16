Imports Metaphor.Processing
Imports TGGD.Presentation

Friend Module CancelChangePaceActivity
    Friend Function Launch(context As IDisplayContext, model As IWorldModel, previous As DialogSource) As DialogSource
        Return Function()
                   model.Avatar.CancelChangePace()
                   Return InPlay.Launch(context, model, previous).Invoke()
               End Function
    End Function
End Module
