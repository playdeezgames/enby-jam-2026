Imports Metaphor.Processing
Imports TGGD.Presentation

Friend Module SetPaceActivity
    Friend Function Launch(c As IDisplayContext, m As IWorldModel, p As DialogSource, pace As Integer) As DialogSource
        Return Function()
                   m.Avatar.SetPace(pace)
                   Return InPlay.Launch(c, m, p).Invoke()
               End Function
    End Function
End Module
