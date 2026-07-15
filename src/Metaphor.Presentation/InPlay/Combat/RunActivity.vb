Imports Metaphor.Processing
Imports TGGD.Presentation

Friend Module RunActivity
    Friend Function LaunchRunActivity(context As IDisplayContext, model As IWorldModel, previous As DialogSource) As DialogSource
        Return Function()
                   model.AttemptRun()
                   Return InPlay.Launch(context, model, previous).Invoke()
               End Function
    End Function
End Module
