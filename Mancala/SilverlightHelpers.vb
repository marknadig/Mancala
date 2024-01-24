Imports System.Runtime.CompilerServices
Imports System
Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Shapes
Imports System.Windows.Media.Imaging
Module SilverlightHelpers
    ' http://blogs.msdn.com/nickkramer/archive/2008/12/06/beginanimation-for-silverlight-2.aspx
    <Extension()> _
    Sub BeginAnimation(ByVal obj As FrameworkElement, ByVal prop As DependencyProperty, ByVal animation As DoubleAnimation)
        Dim storyboard As New Storyboard()
        storyboard.Children.Add(animation)
        storyboard.SetTarget(storyboard, obj)
        storyboard.SetTargetProperty(storyboard, New PropertyPath(prop))
        storyboard.Begin()
    End Sub
End Module

