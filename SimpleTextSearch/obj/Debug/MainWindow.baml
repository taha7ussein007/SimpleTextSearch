   M S B A M L   `   `   `  ����K  GSimpleTextSearch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:SimpleTextSearchSimpleTextSearch  5   	   !    SimpleTextSearch.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   0localclr-namespace:SimpleTextSearch     5   	   J FXamlAnimatedGif, Version=1.1.6.0, Culture=neutral, PublicKeyToken=null<gif2https://github.com/XamlAnimatedGif/XamlAnimatedGif  5   	     � Title$  Simple Text Search��5	   	   $	��500��6$   $	��800��61    � 
ResizeMode$ CanMinimize=�6=    � WindowStartupLocation$ CenterScreen=�6V    � Icon$ ico.ico��6{   .��5
      � .+�5   
   �� -   5         ��Name$ search_textBox��5      $��23��6*   $��10,10,10,10q�66    �� TextWrapping$
 Wrap=�6K   $	��Top=�6_   $��18(�6w    �� TextAlignment$ Center=�6�   
���   � �� 	IsEnabled . ���	#FF0A20FF$��
Tekton Pro)�6�   �� 5   
    J� IsHitTestVisible .  $��Enter Search Query Here��6-   	 �� TextAlignment$	 Center=�6L   $
��Wrap=�6c   $	��Top=�6w   $��Center=�6�   
�������$��20��6�   $	��380��6�   $��205,13,209,0q�6�   ��5      �� 5      
 �� 
TargetType"
 ��6   .�5      ��  �� Property � y� �� Value$ 	Collapsed=�63    �� Triggers 5      �� 5       �� Binding 6&   �� *Text+ �� ElementName$ search_textBox�� �� Value  6[   .a�5      ��  � y�$ Visible=�6;   5      5      5      �� 5   
   -   5         ��Name$ boolSearch_btn��5      $��Boolean Search��6)   $
��Left=�6B   $��
152,38,0,0q�6]   $	��Top=�6q   $	��108��6�   $��24��6�    � Cursor$
 Hand��6�   �� 5   
   -   5        $ inrtdIdxSearch_btn��5      $��Inverted Index Search��6-   $
��Left=�6M   $��
265,38,0,0q�6h   $	��Top=�6|   $	��137��6�   $��24��6�   $
 Hand��6�   �� 5   
   -   5        $ browseFolder_btn��5      $��Select Folders��6+   $��684,427,10,14q�6D   $
 Hand��6t   �� 5   
   -   5         ��Name$ result_lstView��5      $��10,96,10,44q�6+   $
 Hand��6\    �� View 5      �� 5      .P�5      �� $��Document ID��6%    �� Width$ 85��6:    �� DisplayMemberBinding 6E   �� *docId+�� 5      $
��Path��6%   $	 682��63    6?   �� *	docPath+5       5"      �� 5#   
   -   5         ��Name$ 
loadingImg��5#      $
��Left=�6$   '  !XamlAnimatedGif.AnimationBehavior   	SourceUri$ C:\loading.gif?�6?   $��36��6p   $��	18,47,0,0q�6|   $	��Top=�6�   $��51��6�   6�   �� 5$   
   -   5        $ browseFiles_btn��5$      $��Select Files��6*   $��570,427,124,14q�6A   $
 Hand��6w   �� 5%   
   -   5         ��Name$ label��5%      $�� ��6   $
��Left=�6*   $��	10,96,0,0q�6E   $	��Top=�6X   $	��774��6p   $��24��6|   �� 5&   
   -	   5        $ msg_lbl��5&      $�� ��6!   $��10,0,240,13q�6,   |�. $��Tahoma)�6R   
���  ��$��26��6w   $��Bottom=�6�   �� 5'   
   -
   5        $ 
notify_lbl��5'      $�� ��6$   $��	74,56,0,0q�6/   |�. $��Tahoma)�6S   ���	#FF00C5FF$��27��6~   $	��Top=�6�   $
��Left=�6�   $	��178��6�   �� 5(   
   -   5        $ posIdxSearch_btn��5(      $��Positional Index Search��6+   $
��Left=�6M   $��
407,38,0,0q�6h   $	��Top=�6|   $	��137��6�   $��24��6�   $
 Hand��6�   6�   �� 5)   
   -   5        $ tfSearch_btn��5)      $��TF-IDF Search��6'   $
��Left=�6?   $��
549,38,0,0q�6Z   $	��Top=�6n   $	��108��6�   $��24��6�   $
 Hand��6�   5*      5+      