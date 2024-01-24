Imports System.Windows.Controls

Class bucketInfo
    Private Shared nextIndex As Integer = 0
    Public index As Integer
    Private button As bucket
    Public forPlayer As Integer
    Public isMancala As Boolean
    Public opposite As Integer

    Public Sub New(ByVal button As bucket, ByVal forPlayer As Integer, ByVal opposite As Integer)
        Me.button = button
        button.Count = 0
        button.IsEnabled = False
        Me.forPlayer = forPlayer
        Me.isMancala = False
        Me.opposite = opposite
        ' number sequentiall so we can keep track 
        Me.index = bucketInfo.nextIndex
        bucketInfo.nextIndex += 1
        AddHandler button.Clicked, AddressOf Button_Click
    End Sub

    Public Sub New(ByVal button As bucket, ByVal forPlayer As Integer)
        Me.button = button
        button.IsEnabled = False
        button.Count = 0
        Me.forPlayer = forPlayer
        Me.isMancala = True
        Me.opposite = 0
        ' number sequentiall so we can keep track 
        Me.index = bucketInfo.nextIndex
        bucketInfo.nextIndex += 1
    End Sub

    Public Property Count() As Integer
        Get
            Return button.Count
        End Get
        Set(ByVal value As Integer)
            button.Count = value
        End Set
    End Property

    Public Property IsEnabled() As Boolean
        Get
            Return button.IsEnabled
        End Get
        Set(ByVal value As Boolean)
            button.IsEnabled = value
        End Set
    End Property

    Private Sub Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        RaiseEvent Bucketclicked(Me)
    End Sub

    Public Shared Event Bucketclicked(ByVal bi As bucketInfo)

End Class


