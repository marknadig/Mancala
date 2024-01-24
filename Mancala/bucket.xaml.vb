Imports System
Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Shapes
Imports System.Windows.Media.Imaging

Partial Public Class bucket 
    Inherits UserControl
    Private Shared r As New Random(CInt(DateTime.Now.Millisecond))
    Private center As Point
    Public Shared animate As Boolean = False


    Private Shared freeMarbles As New List(Of Image)
    Private myMarbles As New List(Of Image)
    Private _count As Integer
    Public Property Count() As Integer
        Get
            Return _count
        End Get
        Set(ByVal value As Integer)
            If value = _count Then Exit Property
            ' If reduce, then pop off
            Dim marble As Image
            For i As Integer = _count - 1 To value Step -1
                If myMarbles.Count > 0 Then
                    marble = myMarbles(myMarbles.Count - 1)
                    myMarbles.Remove(marble)
                    LayoutRoot.Children.Remove(marble)
                    freeMarbles.Add(marble)
                End If
            Next

            ' if increase, then add
            Dim top, left As Integer
            For i As Integer = _count To value - 1

                marble = freeMarbles(freeMarbles.Count - 1)
                freeMarbles.Remove(marble)
                myMarbles.Add(marble)

                LayoutRoot.Children.Add(marble)
                top = Math.Min( _
                    CInt((r.NextDouble * (Me.bucketBorder.Height - marble.Height))) _
                       , Me.bucketBorder.Height - marble.Height)
                top = Math.Max(top, marble.Height - 5)

                left = Math.Min( _
                    CInt((r.NextDouble * (Me.bucketBorder.Width - marble.Width))) _
                    , Me.bucketBorder.Width - marble.Width)
                left = Math.Max(left, marble.Width - 5)


                If animate Then
                    Dim dax, day As New DoubleAnimation()
                    dax.Duration = TimeSpan.FromMilliseconds(300)
                    dax.To = left
                    dax.From = bucketBorder.Width + 20

                    day.Duration = dax.Duration
                    day.To = top
                    day.From = -20 ' from top right of bucket
                    marble.BeginAnimation(Canvas.LeftProperty, dax)
                    marble.BeginAnimation(Canvas.TopProperty, day)

                Else
                    Canvas.SetTop(marble, top)
                    Canvas.SetLeft(marble, left)
                End If



                ' System.Diagnostics.Debug.WriteLine(String.Format("Top {0} Left {1}", top, left))
            Next

            _count = value
            bucketCount.Text = If(value = 0, String.Empty, value.ToString())

        End Set
    End Property


    Public Sub New()
        ' Required to initialize variables
        InitializeComponent()
        bucketCount.Text = String.Empty

        'bucketBorder.Height = Me.Height

    End Sub

    Shared Sub New()
        ' shared ctor to init marble collection - only will have 
        ' todo: add marble images
        For i As Integer = 1 To 96 ' do double 48 so we know there's enough free when we need them
            Dim imageName As String = String.Format("marble{0}.png", Math.Min(Math.Max(CInt(r.NextDouble() * 10), 1), 10))
            Dim marble As New Image
            marble.Source = New BitmapImage(New Uri(imageName, UriKind.Relative))
            marble.Height = 15
            marble.Width = 15
            freeMarbles.Add(marble)
        Next
    End Sub


    Public Event Clicked(ByVal sender As Object, ByVal e As System.Windows.Input.MouseEventArgs)

    Private Sub bucket_MouseEnter(ByVal sender As Object, ByVal e As System.Windows.Input.MouseEventArgs) Handles Me.MouseEnter
        If Me.IsEnabled Then MouseOverAnimation.Begin()
    End Sub

    Private Sub bucket_MouseLeftButtonDown(ByVal sender As Object, ByVal e As System.Windows.Input.MouseButtonEventArgs) Handles Me.MouseLeftButtonDown
        RaiseEvent Clicked(sender, e)
    End Sub

    Private Sub bucket_SizeChanged(ByVal sender As Object, ByVal e As System.Windows.SizeChangedEventArgs) Handles Me.SizeChanged
        Me.bucketBorder.StrokeThickness = 0
        Me.bucketBorder.Height = Me.Height
        Canvas.SetTop(Me.bucketCount, Me.Height - 5)
        Me.center = New Point(CInt(Me.bucketBorder.Width / 2), CInt(Me.bucketBorder.Height / 2))
    End Sub
End Class

