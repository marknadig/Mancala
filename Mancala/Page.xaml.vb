Imports System
Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Shapes
Imports Microsoft.VisualBasic

Partial Public Class Page 
    Inherits UserControl

    Dim player As Integer
    Dim turns As List(Of String)
    Dim buckets As New List(Of bucketInfo)

    ' these for moving from bucket to bucket using timer instead of locking up UI thread
    Dim movingMarbles As Integer
    Dim currentBucket As bucketInfo
    Dim WithEvents dt As New System.Windows.Threading.DispatcherTimer

    Public Sub New()
        ' Required to initialize variables
        InitializeComponent()
        buckets.Add(New bucketInfo(player2Mancala, 2))
        buckets.Add(New bucketInfo(bucket1, 2, 6))
        buckets.Add(New bucketInfo(bucket2, 2, 5))
        buckets.Add(New bucketInfo(bucket3, 2, 4))
        buckets.Add(New bucketInfo(bucket4, 1, 3))
        buckets.Add(New bucketInfo(bucket5, 1, 2))
        buckets.Add(New bucketInfo(bucket6, 1, 1))
        buckets.Add(New bucketInfo(player1Mancala, 1))
        buckets.Add(New bucketInfo(bucket7, 1, 13))
        buckets.Add(New bucketInfo(bucket8, 1, 12))
        buckets.Add(New bucketInfo(bucket9, 1, 11))
        buckets.Add(New bucketInfo(bucket10, 2, 10))
        buckets.Add(New bucketInfo(bucket11, 2, 9))
        buckets.Add(New bucketInfo(bucket12, 2, 8))

        Me.commentary.Text = String.Empty
        AddHandler bucketInfo.Bucketclicked, AddressOf StartTurn

        dt.Interval = New TimeSpan(0, 0, 1) ' 1s

    End Sub

    Private Sub Comment(ByVal cmt As String)
        Me.commentary.Text &= vbCrLf & turns.Count.ToString & ". " & cmt
    End Sub

    Private Sub Status(Optional ByVal text As String = "")
        Me.lblStatus.Text = text
    End Sub

    Private Function PlayerName() As String
        If player = 1 Then
            Return Me.player1Name.Text
        Else
            Return Me.player2Name.Text
        End If
    End Function

    Private Sub SwitchPlayer(Optional ByVal noNextTurn As Boolean = False)
        If player = 0 OrElse player = 2 Then ' first time or
            player = 1
        Else
            player = 2
        End If
        If Not noNextTurn Then NextTurn()
    End Sub

    Private Sub refreshPlayerState()
        For Each bi As bucketInfo In buckets
            If bi.isMancala OrElse movingMarbles > 0 Then
                ' Disable while in turn
                bi.IsEnabled = False
            Else
                bi.IsEnabled = (bi.forPlayer = player)
            End If
        Next
    End Sub

    Private Sub NextTurn()
        refreshPlayerState()
        Comment(String.Format("{0}'s turn.", PlayerName))
    End Sub

    Private Sub Undo()
        If turns.Count = 0 Then
            msgbox("No more to undo.") : Exit Sub
        End If
        ' clean commentary for each turn
        Me.commentary.Text = String.Empty

        Dim turn As Integer = turns.Count - 1
        Dim s As String = turns(turn)
        turns.Removeat(turn)
        Dim parts() As String = s.Split("|"c)
        Integer.TryParse(parts(0), player)
        For i As Integer = 0 To buckets.Count - 1
            buckets(i).Count = CInt(parts(i + 1))
        Next
        refreshPlayerState()
        Comment("Undone")
    End Sub

    Private Sub StartTurn(ByVal bi As bucketInfo)
        If bi.Count = 0 Then
            msgbox("Please click a bucket with marbles.") : Exit Sub
        End If
        ' clean commentary for each turn
        Me.commentary.Text = String.Empty

        ' save off turn state
        Dim s As String = player.ToString(0)
        For i As Integer = 0 To buckets.Count - 1
            s &= "|" & buckets(i).Count.ToString
        Next
        turns.Add(s)

        Dim marbles As Integer = GrabMarbles(bi)
        Comment("Picked up " & marbles.ToString)
        click_mp3.Play() : click_mp3.Position = New TimeSpan(0)

        ' Queue up and let timer handle
        movingMarbles = marbles
        currentBucket = bi
        refreshPlayerState()
        dt.Start()
    End Sub


    Private Sub MoveOneMarbleForTurn(ByVal sender As Object, ByVal e As System.EventArgs) Handles dt.Tick
        If movingMarbles <= 0 Then
            dt.Stop()
            Exit Sub
        End If ' failsafe

        currentBucket = GetNextBucket(currentBucket)

        ' if my mancala - or any other bucket, drop one off
        If currentBucket.isMancala AndAlso currentBucket.forPlayer <> player Then
            currentBucket = GetNextBucket(currentBucket)
        End If
        DropMarble(currentBucket)

        movingMarbles -= 1
        If movingMarbles = 0 Then FinishTurn()
    End Sub

    Private Sub FinishTurn()
        dt.Stop()

        ' If last marble was in My empty bucket, then grab all the others from the opposite
        If currentBucket.Count = 1 AndAlso Not currentBucket.isMancala AndAlso currentBucket.forPlayer = player Then
            GrabFromOpposite(currentBucket)
        End If

        ' If dropping last marble, see if any left on my side. If not, game over.
        If Not AnyLeft() Then
            Comment("No more left for " & PlayerName())
            ' grab all of them
            Dim marbles As Integer = 0
            For Each left As bucketInfo In buckets
                marbles += GrabMarbles(left)
            Next
            If marbles > 0 Then
                SwitchPlayer(True)
                Comment("Grabbing " & marbles.ToString & " marbles for other.")
                DropInMancala(marbles)
                Comment("Game over!")
                Exit Sub
            End If
        End If

        ' if last one in my mancala, then go again!
        If currentBucket.isMancala Then
            Status(String.Format("{0} goes again!", PlayerName))
            Comment(String.Format("Last one in Mancala, {0} goes again!", PlayerName))
            NextTurn()
        Else
            Status()
            SwitchPlayer()
        End If
    End Sub




    Private Function AnyLeft() As Boolean
        For Each bi As bucketInfo In buckets
            If bi.forPlayer = player And Not bi.isMancala Then
                ' see if any
                If bi.Count > 0 Then Return True
            End If
        Next

        Return False
    End Function

    Private Sub GrabFromOpposite(ByVal bi As bucketInfo)
        Dim marbles As Integer
        ' grab from orignal cup
        marbles = GrabMarbles(bi)
        ' grab from opposite
        marbles += GrabMarbles(buckets(bi.opposite))

        DropInMancala(marbles)
        Comment("Last marble in my empty - grabbing from opposite.")
    End Sub

    Private Function GrabMarbles(ByVal bi As bucketInfo) As Integer
        If bi.isMancala Then Return 0 ' short circuit
        Dim ret As Integer = bi.Count
        bi.Count = 0 ' clear to 0   
        Return ret
    End Function

    Private Sub DropMarble(ByVal bi As bucketInfo, Optional ByVal marbles As Integer = 1)
        bi.Count += marbles
        drop_mp3.Play() : drop_mp3.Position = New TimeSpan(0)
    End Sub

    Private Sub DropInMancala(ByVal marbles As Integer)
        ' assmes current player always
        If player = 1 Then
            DropMarble(buckets(7), marbles)
        Else
            DropMarble(buckets(0), marbles)
        End If
    End Sub

    Private Function GetNextBucket(ByVal bi As bucketInfo) As bucketInfo
        Dim i As Integer = bi.index + 1
        If i = buckets.Count Then
            i = 0
        End If
        Return buckets(i)
    End Function

    Private Sub btnUndo_Click(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnUndo.Click
        Undo()
    End Sub

    Public Sub msgbox(ByVal message As String)
        System.Windows.Browser.HtmlPage.Window.Alert(message)
    End Sub

    Private Sub Page_Loaded(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles Me.Loaded
        ' SwitchPlayer()
    End Sub

    Private Sub btnStart_Click(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnStart.Click
        If player1Name.Text.Length = 0 Then
            player1Name.Focus()
            Status("Please enter player name.")
            Exit Sub
        End If
        If player2Name.Text.Length = 0 Then
            player2Name.Focus()
            Status("Please enter player name.")
            Exit Sub
        End If

        If btnStart.Content.ToString = "Start" Then
            ShowBoard.Begin()
            btnUndo.IsEnabled = True
            btnStart.Content = "Restart"
        End If

        ' start/restart game
        player = 0
        turns = New List(Of String)
        Status()
        commentary.Text = String.Empty
        ' two pass - first pick up all marbles (due to how bucket is keeping track of just 48)
        bucket.animate = False
        For Each bi As bucketInfo In buckets
            bi.Count = If( bi.isMancala , 0, 4)
        Next
        bucket.animate = True
        SwitchPlayer()
    End Sub

End Class
