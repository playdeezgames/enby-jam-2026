Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Friend Module WorldExtensions
    <Extension>
    Friend Function IsInCombat(world As IWorld) As Boolean
        Dim character = world.Avatar
        Return Not character.IsDead() AndAlso character.Location.GetOtherCharacters(character).Any(Function(x) x.HasTag(Tags.ENEMY))
    End Function
End Module
