
Imports Metaphor.Processing
Imports TGGD.Presentation

Friend Module AttackActivity
    Friend Function LaunchAttackActivity(c As IDisplayContext, m As IWorldModel, p As DialogSource, characterModel As ICharacterModel) As DialogSource
        Return Function()
                   characterModel.Attack()
                   Return InPlay.Launch(c, m, p).Invoke()
               End Function
    End Function
End Module
