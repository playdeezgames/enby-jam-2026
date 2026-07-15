Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Friend Module EntityExtensions
    <Extension>
    Friend Sub SetName(entity As IKJEntity, name As String)
        entity.SetMetadata(Metadatas.NAME, name)
    End Sub
    <Extension>
    Friend Sub SetFlavor(entity As IKJEntity, description As String)
        entity.SetMetadata(Metadatas.FLAVOR, description)
    End Sub
    <Extension>
    Friend Function GetName(entity As IKJEntity) As String
        Return entity.GetMetadata(Metadatas.NAME)
    End Function
    <Extension>
    Friend Function GetFlavor(entity As IKJEntity) As String
        Return entity.GetMetadata(Metadatas.FLAVOR)
    End Function
    <Extension>
    Friend Sub InitializeCounter(entity As IKJEntity, counterId As String, value As Integer, minimum As Integer, maximum As Integer)
        entity.SetCounterMinimum(counterId, minimum)
        entity.SetCounterMaximum(counterId, maximum)
        entity.SetCounter(counterId, value)
    End Sub
End Module
