using System;
using System.Collections;
using System.Net;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Database;
using MediaPortal.Music.Database;
using MediaPortal.TagReader;
using MediaPortal.Dialogs;

namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class GUIMusicArtists: GUIWindow, IComparer
  { 
    enum Controls
    {
      CONTROL_BTNVIEWASICONS=   2,
      CONTROL_BTNSORTBY   =     3,
      CONTROL_BTNSORTASC  =     4,
      CONTROL_BTNTYPE    =      6,
      CONTROL_BTNPLAYLISTS=     7,
      CONTROL_BTNSCAN     =     9,
      CONTROL_BTNREC      =     10,
      CONTROL_LIST        =     50,
      CONTROL_THUMBS      =     51,
      CONTROL_ALBUMS      =     52,
      CONTROL_LABELFILES  =       12,
      CONTROL_LABEL=       15,
		CONTROL_SEARCH = 8

    };
    #region Base variabeles
    enum SortMethod
    {
      SORT_NAME=0,
      SORT_DATE=1,
      SORT_SIZE=2,
      SORT_TRACK=3,
      SORT_DURATION=4,
      SORT_TITLE=5,
      SORT_ARTIST=6,
      SORT_ALBUM=7,
      SORT_FILENAME=8,
			SORT_RATING=9
    }

    enum View
    {
      VIEW_AS_LIST    =       0,
      VIEW_AS_ICONS    =      1,
      VIEW_AS_LARGEICONS  =   2,
    }
    enum Mode
    {
      ShowArtists,
      ShowAlbums,
      ShowSongs
    }
    View              currentView=View.VIEW_AS_LIST;
    SortMethod        currentSortMethod=SortMethod.SORT_TITLE;
    View              currentViewRoot=View.VIEW_AS_LIST;
    SortMethod        currentSortMethodRoot=SortMethod.SORT_NAME;
    bool              m_bSortAscending=true;
    bool              m_bSortAscendingRoot=true;

    DirectoryHistory  m_history = new DirectoryHistory();
    string            m_strDirectory="";
    int               m_iItemSelected=-1;   
    VirtualDirectory  m_directory = new VirtualDirectory();
    #endregion
    MusicDatabase          m_database = new MusicDatabase();
    Mode              m_Mode=Mode.ShowArtists;
    string            m_strArtist="";
    string            m_strAlbum="";

    public GUIMusicArtists()
    {
      GetID=(int)GUIWindow.Window.WINDOW_MUSIC_ARTIST;
      
      m_directory.AddDrives();
      m_directory.SetExtensions (Utils.AudioExtensions);
      LoadSettings();
    }
    ~GUIMusicArtists()
    {
    }

    public override bool Init()
    {
      m_strDirectory="";
      return Load (GUIGraphicsContext.Skin+@"\mymusicartists.xml");
    }

    #region Serialisation
    void LoadSettings()
    {
      using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        string strTmp="";
        strTmp=(string)xmlreader.GetValue("musicartist","viewby");
        if (strTmp!=null)
        {
          if (strTmp=="list") currentView=View.VIEW_AS_LIST;
          else if (strTmp=="icons") currentView=View.VIEW_AS_ICONS;
          else if (strTmp=="largeicons") currentView=View.VIEW_AS_LARGEICONS;
        }
        strTmp=(string)xmlreader.GetValue("musicartist","viewbyroot");
        if (strTmp!=null)
        {
          if (strTmp=="list") currentViewRoot=View.VIEW_AS_LIST;
          else if (strTmp=="icons") currentViewRoot=View.VIEW_AS_ICONS;
          else if (strTmp=="largeicons") currentViewRoot=View.VIEW_AS_LARGEICONS;
        }

        strTmp=(string)xmlreader.GetValue("musicartist","sort");
        if (strTmp!=null)
        {
          if (strTmp=="album") currentSortMethod=SortMethod.SORT_ALBUM;
          else if (strTmp=="artist") currentSortMethod=SortMethod.SORT_ARTIST;
          else if (strTmp=="name") currentSortMethod=SortMethod.SORT_NAME;
          else if (strTmp=="date") currentSortMethod=SortMethod.SORT_DATE;
          else if (strTmp=="size") currentSortMethod=SortMethod.SORT_SIZE;
          else if (strTmp=="track") currentSortMethod=SortMethod.SORT_TRACK;
          else if (strTmp=="duration") currentSortMethod=SortMethod.SORT_DURATION;
          else if (strTmp=="filename") currentSortMethod=SortMethod.SORT_FILENAME;
					else if (strTmp=="title") currentSortMethod=SortMethod.SORT_TITLE;
					else if (strTmp=="rating") currentSortMethod=SortMethod.SORT_RATING;
        }
        strTmp=(string)xmlreader.GetValue("musicartist","sortroot");
        if (strTmp!=null)
        {
          if (strTmp=="album") currentSortMethodRoot=SortMethod.SORT_ALBUM;
          else if (strTmp=="artist") currentSortMethodRoot=SortMethod.SORT_ARTIST;
          else if (strTmp=="name") currentSortMethodRoot=SortMethod.SORT_NAME;
          else if (strTmp=="date") currentSortMethodRoot=SortMethod.SORT_DATE;
          else if (strTmp=="size") currentSortMethodRoot=SortMethod.SORT_SIZE;
          else if (strTmp=="track") currentSortMethodRoot=SortMethod.SORT_TRACK;
          else if (strTmp=="duration") currentSortMethodRoot=SortMethod.SORT_DURATION;
          else if (strTmp=="filename") currentSortMethodRoot=SortMethod.SORT_FILENAME;
					else if (strTmp=="title") currentSortMethodRoot=SortMethod.SORT_TITLE;
					else if (strTmp=="rating") currentSortMethodRoot=SortMethod.SORT_RATING;
        }

        m_bSortAscending=xmlreader.GetValueAsBool("musicartist","sortascending",true);
        m_bSortAscendingRoot=xmlreader.GetValueAsBool("musicartist","sortascendingroot",true);
      }

    }
    
    void SaveSettings()
    {
      using(AMS.Profile.Xml   xmlwriter=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        switch (currentView)
        {
          case View.VIEW_AS_LIST: 
            xmlwriter.SetValue("musicartist","viewby","list");
            break;
          case View.VIEW_AS_ICONS: 
            xmlwriter.SetValue("musicartist","viewby","icons");
            break;
          case View.VIEW_AS_LARGEICONS: 
            xmlwriter.SetValue("musicartist","viewby","largeicons");
            break;
        }
        switch (currentViewRoot)
        {
          case View.VIEW_AS_LIST: 
            xmlwriter.SetValue("musicartist","viewbyroot","list");
            break;
          case View.VIEW_AS_ICONS: 
            xmlwriter.SetValue("musicartist","viewbyroot","icons");
            break;
          case View.VIEW_AS_LARGEICONS: 
            xmlwriter.SetValue("musicartist","viewbyroot","largeicons");
            break;
        }
        switch (currentSortMethod)
        {
          case SortMethod.SORT_NAME:
            xmlwriter.SetValue("musicartist","sort","name");
            break;
          case SortMethod.SORT_DATE:
            xmlwriter.SetValue("musicartist","sort","date");
            break;
          case SortMethod.SORT_SIZE:
            xmlwriter.SetValue("musicartist","sort","size");
            break;
          case SortMethod.SORT_TRACK:
            xmlwriter.SetValue("musicartist","sort","track");
            break;
          case SortMethod.SORT_DURATION:
            xmlwriter.SetValue("musicartist","sort","duration");
            break;
          case SortMethod.SORT_TITLE:
            xmlwriter.SetValue("musicartist","sort","title");
            break;
          case SortMethod.SORT_ALBUM:
            xmlwriter.SetValue("musicartist","sort","album");
            break;
          case SortMethod.SORT_ARTIST:
            xmlwriter.SetValue("musicartist","sort","artist");
            break;
          case SortMethod.SORT_FILENAME:
            xmlwriter.SetValue("musicartist","sort","filename");
						break;
					case SortMethod.SORT_RATING:
						xmlwriter.SetValue("musicartist","sort","rating");
						break;
        }
        switch (currentSortMethodRoot)
        {
          case SortMethod.SORT_NAME:
            xmlwriter.SetValue("musicartist","sortroot","name");
            break;
          case SortMethod.SORT_DATE:
            xmlwriter.SetValue("musicartist","sortroot","date");
            break;
          case SortMethod.SORT_SIZE:
            xmlwriter.SetValue("musicartist","sortroot","size");
            break;
          case SortMethod.SORT_TRACK:
            xmlwriter.SetValue("musicartist","sortroot","track");
            break;
          case SortMethod.SORT_DURATION:
            xmlwriter.SetValue("musicartist","sortroot","duration");
            break;
          case SortMethod.SORT_TITLE:
            xmlwriter.SetValue("musicartist","sortroot","title");
            break;
          case SortMethod.SORT_ALBUM:
            xmlwriter.SetValue("musicartist","sortroot","album");
            break;
          case SortMethod.SORT_ARTIST:
            xmlwriter.SetValue("musicartist","sortroot","artist");
            break;
          case SortMethod.SORT_FILENAME:
            xmlwriter.SetValue("musicartist","sortroot","filename");
						break;
					case SortMethod.SORT_RATING:
						xmlwriter.SetValue("musicartist","sortroot","rating");
						break;
        }

        xmlwriter.SetValueAsBool("musicartist","sortascending",m_bSortAscending);
        xmlwriter.SetValueAsBool("musicartist","sortascendingroot",m_bSortAscendingRoot);
      }
    }
    #endregion

    #region BaseWindow Members
    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PARENT_DIR)
      {
        GUIListItem item = GetItem(0);
        if (item!=null)
        {
          if (item.IsFolder && item.Label=="..")
          {
            switch (m_Mode)
            {
              case Mode.ShowSongs:
                LoadDirectory(m_strDirectory,Mode.ShowAlbums);
                break;
              case Mode.ShowAlbums:
                LoadDirectory(m_strDirectory,Mode.ShowArtists);
                break;
            }
          }
        }
        return;
      }

      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
			{
				GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_HOME);
        return;
      }
      if (action.wID==Action.ActionType.ACTION_SHOW_PLAYLIST)
      {
        GUIWindowManager.ActivateWindow( (int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST);
        return;
      }

      if (action.wID == Action.ActionType.ACTION_CONTEXT_MENU)
      {
        ShowContextMenu();
      }
      base.OnAction(action);
    }

    
    public override bool OnMessage(GUIMessage message)
    {
      switch ( message.Message )
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
					base.OnMessage(message);
					LoadSettings();
          
          ShowThumbPanel();
          LoadDirectory(m_strDirectory,m_Mode);
          return true;

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
					if (GUIMusicFiles.IsMusicWindow(message.Param1))
					{
						MusicState.StartWindow=message.Param1;
					}
          m_iItemSelected=GetSelectedItemNo();
          SaveSettings();
          break;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          int iControl=message.SenderControlId;
          if (iControl==(int)Controls.CONTROL_BTNVIEWASICONS)
          {
            if (m_Mode==Mode.ShowArtists)
            {
              switch (currentViewRoot)
              {
                case View.VIEW_AS_LIST:
                  currentViewRoot=View.VIEW_AS_ICONS;
                  break;
                case View.VIEW_AS_ICONS:
                  currentViewRoot=View.VIEW_AS_LARGEICONS;
                  break;
                case View.VIEW_AS_LARGEICONS:
                  currentViewRoot=View.VIEW_AS_LIST;
                  break;
              }
            }
            else
            {
              switch (currentView)
              {
                case View.VIEW_AS_LIST:
                  currentView=View.VIEW_AS_ICONS;
                  break;
                case View.VIEW_AS_ICONS:
                  if (m_Mode==Mode.ShowAlbums)
                     currentView=View.VIEW_AS_LIST;
                  else
                     currentView=View.VIEW_AS_LARGEICONS;
                  break;
                case View.VIEW_AS_LARGEICONS:
                  currentView=View.VIEW_AS_LIST;
                  break;
              }
            }
            //LoadDirectory(m_strDirectory,m_Mode);
            ShowThumbPanel();
            GUIControl.FocusControl(GetID,iControl);
          }
			  // search-button handling
			if (iControl == (int)Controls.CONTROL_SEARCH)
			{
				int activeWindow=(int)GUIWindowManager.ActiveWindow;
				VirtualSearchKeyboard keyBoard=(VirtualSearchKeyboard)GUIWindowManager.GetWindow(1001);
				keyBoard.Text = "";
				keyBoard.Reset();
				keyBoard.TextChanged+=new MediaPortal.Dialogs.VirtualSearchKeyboard.TextChangedEventHandler(keyboard_TextChanged); // add the event handler
				keyBoard.DoModal(activeWindow); // show it...
				keyBoard.TextChanged-=new MediaPortal.Dialogs.VirtualSearchKeyboard.TextChangedEventHandler(keyboard_TextChanged);	// remove the handler			
				System.GC.Collect(); // collect some garbage			
			}
			  //
          if (iControl==(int)Controls.CONTROL_BTNSORTASC)
          {
            if (m_Mode==Mode.ShowArtists)
              m_bSortAscendingRoot=!m_bSortAscendingRoot;
            else
              m_bSortAscending=!m_bSortAscending;
            OnSort();
            UpdateButtons();
            GUIControl.FocusControl(GetID,iControl);
          }


          if (iControl==(int)Controls.CONTROL_BTNSORTBY) // sort by
          {
            if (m_Mode==Mode.ShowArtists)
            {
                  currentSortMethodRoot=SortMethod.SORT_NAME;
            }
            else
            {
              switch (currentSortMethod)
              {
                case SortMethod.SORT_NAME:
                  currentSortMethod=SortMethod.SORT_DATE;
                  break;
                case SortMethod.SORT_DATE:
                  currentSortMethod=SortMethod.SORT_SIZE;
                  break;
                case SortMethod.SORT_SIZE:
                  currentSortMethod=SortMethod.SORT_TRACK;
                  break;
                case SortMethod.SORT_TRACK:
                  currentSortMethod=SortMethod.SORT_DURATION;
                  break;
                case SortMethod.SORT_DURATION:
                  currentSortMethod=SortMethod.SORT_TITLE;
                  break;
                case SortMethod.SORT_TITLE:
                  currentSortMethod=SortMethod.SORT_ALBUM;
                  break;
                case SortMethod.SORT_ALBUM:
                  currentSortMethod=SortMethod.SORT_FILENAME;
                  break;
                case SortMethod.SORT_FILENAME:
                  currentSortMethod=SortMethod.SORT_RATING;
									break;
								case SortMethod.SORT_RATING:
									currentSortMethod=SortMethod.SORT_NAME;
									break;
              }
            }
            OnSort();
            GUIControl.FocusControl(GetID,iControl);
          }
          
          if (iControl==(int)Controls.CONTROL_BTNTYPE)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
            OnMessage(msg);         
            int nSelected=(int)msg.Param1;
            int nNewWindow=(int)GUIWindow.Window.WINDOW_MUSIC_ARTIST;
            switch (nSelected)
            {
              case 0: //  files
                nNewWindow=(int)GUIWindow.Window.WINDOW_MUSIC_FILES;
                break;
              case 1: //  albums
                nNewWindow=(int)GUIWindow.Window.WINDOW_MUSIC_ALBUM;
                break;
              case 2: //  artist
                nNewWindow=(int)GUIWindow.Window.WINDOW_MUSIC_ARTIST;
                break;
              case 3: //  genres
                nNewWindow=(int)GUIWindow.Window.WINDOW_MUSIC_GENRE;
                break;
              case 4: //  top100
                nNewWindow=(int)GUIWindow.Window.WINDOW_MUSIC_TOP100;
								break;
							case 5 : //	favorites
								nNewWindow = (int)GUIWindow.Window.WINDOW_MUSIC_FAVORITES;
								break;
            }

            if (nNewWindow!=GetID)
            {
              MusicState.StartWindow=nNewWindow;
              GUIWindowManager.ActivateWindow(nNewWindow);
            }

            return true;
          }
          if (iControl==(int)Controls.CONTROL_THUMBS||iControl==(int)Controls.CONTROL_LIST || iControl==(int)Controls.CONTROL_ALBUMS)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
            OnMessage(msg);         
            int iItem=(int)msg.Param1;
            int iAction=(int)message.Param1;
            if (iAction == (int)Action.ActionType.ACTION_SHOW_INFO) 
            {
              OnInfo(iItem);
              LoadDirectory(m_strDirectory,m_Mode);
            }
            if (iAction == (int)Action.ActionType.ACTION_SELECT_ITEM)
            {
              OnClick(iItem);
            }
            if (iAction == (int)Action.ActionType.ACTION_QUEUE_ITEM)
            {
              OnQueueItem(iItem);
            }

          }
          break;
      }
      return base.OnMessage(message);
    }


    bool ViewByIcon
    {
      get 
      {
        if (m_Mode==Mode.ShowArtists)
        {
          if (currentViewRoot != View.VIEW_AS_LIST) return true;
        }
        else
        {
          if (currentView != View.VIEW_AS_LIST) return true;
        }
        return false;
      }
    }

    bool ViewByLargeIcon
    {
      get
      {
        if (m_Mode==Mode.ShowArtists)
        {
          if (currentViewRoot == View.VIEW_AS_LARGEICONS) return true;
        }
        else
        {
          if (currentView == View.VIEW_AS_LARGEICONS) return true;
        }
        return false;
      }
    }

    void ShowContextMenu()
    {
      GUIListItem item=GetSelectedItem();
      int itemNo=GetSelectedItemNo();
      if (item==null) return;

      GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg==null) return;
      dlg.Reset();
      dlg.SetHeading(924); // menu

      dlg.Add( GUILocalizeStrings.Get(928)); //IMDB
      dlg.Add( GUILocalizeStrings.Get(208)); //play
      dlg.Add( GUILocalizeStrings.Get(926)); //Queue
			dlg.Add( GUILocalizeStrings.Get(136)); //PlayList
			if (!item.IsFolder && !item.IsRemote)
			{
				dlg.AddLocalizedString(930); //Add to favorites
				dlg.AddLocalizedString(931); //Rating
			}

      dlg.DoModal( GetID);
      if (dlg.SelectedLabel==-1) return;
      switch (dlg.SelectedLabel)
      {
        case 0: // IMDB
          OnInfo(itemNo);
          break;

        case 1: // play
          OnClick(itemNo);	
          break;
					
        case 2: // add to playlist
          OnQueueItem(itemNo);	
          break;
					
        case 3: // show playlist
          GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST);
					break;
				case 4: // add to favorites
					m_database.AddSongToFavorites(item.Path);
					break;
				case 5:// Rating
					OnSetRating(GetSelectedItemNo());
					break;
      }
    }

		void OnSetRating(int itemNumber)
		{
			GUIListItem item = GetItem(itemNumber);
			if (item ==null) return;
			GUIDialogSetRating dialog = (GUIDialogSetRating)GUIWindowManager.GetWindow( (int)GUIWindow.Window.WINDOW_DIALOG_RATING);
			if (item.MusicTag!=null) 
			{
				dialog.Rating=((MusicTag)item.MusicTag).Rating;
				dialog.SetTitle(String.Format("{0}-{1}", ((MusicTag)item.MusicTag).Artist, ((MusicTag)item.MusicTag).Title) );
			}
			dialog.FileName=item.Path;
			dialog.DoModal(GetID);
			if (item.MusicTag!=null) 
			{
				((MusicTag)item.MusicTag).Rating=dialog.Rating;
			}
			m_database.SetRating(item.Path,dialog.Rating);
			if (dialog.Result == GUIDialogSetRating.ResultCode.Previous)
			{
				while (itemNumber >0)
				{
					itemNumber--;
					item = GetItem(itemNumber);
					if (!item.IsFolder && !item.IsRemote)
					{
						OnSetRating(itemNumber);
						return;
					}
				}
			}

			if (dialog.Result == GUIDialogSetRating.ResultCode.Next)
			{
				while (itemNumber+1 < GetItemCount() )
				{
					itemNumber++;
					item = GetItem(itemNumber);
					if (!item.IsFolder && !item.IsRemote)
					{
						OnSetRating(itemNumber);
						return;
					}
				}
			}
		}
    GUIListItem GetSelectedItem()
    {
      int iControl;
      if (ViewByIcon)
      {
        if (m_Mode==Mode.ShowAlbums)
          iControl=(int)Controls.CONTROL_ALBUMS;
        else 
          iControl=(int)Controls.CONTROL_THUMBS;
      }
      else
        iControl=(int)Controls.CONTROL_LIST;
      GUIListItem item = GUIControl.GetSelectedListItem(GetID,iControl);
      return item;
    }

    GUIListItem GetItem(int iItem)
    {
      int iControl;
      if (ViewByIcon)
      {
        if (m_Mode==Mode.ShowAlbums)
          iControl=(int)Controls.CONTROL_ALBUMS;
        else 
          iControl=(int)Controls.CONTROL_THUMBS;
      }
      else
        iControl=(int)Controls.CONTROL_LIST;
      GUIListItem item = GUIControl.GetListItem(GetID,iControl,iItem);
      return item;
    }

    int GetSelectedItemNo()
    {
      int iControl;
      if (ViewByIcon)
      {
        if (m_Mode==Mode.ShowAlbums)
          iControl=(int)Controls.CONTROL_ALBUMS;
        else 
          iControl=(int)Controls.CONTROL_THUMBS;
      }
      else
        iControl=(int)Controls.CONTROL_LIST;

      GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
      OnMessage(msg);         
      int iItem=(int)msg.Param1;
      return iItem;
    }
    int GetItemCount()
    {
      int iControl;
      if (ViewByIcon)
      {
        if (m_Mode==Mode.ShowAlbums)
          iControl=(int)Controls.CONTROL_ALBUMS;
        else 
          iControl=(int)Controls.CONTROL_THUMBS;
      }
      else
        iControl=(int)Controls.CONTROL_LIST;

      return GUIControl.GetItemCount(GetID,iControl);
    }

    void UpdateButtons()
    {
      GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_BTNTYPE,MusicState.StartWindow-(int)GUIWindow.Window.WINDOW_MUSIC_FILES);

      GUIControl.HideControl(GetID,(int)Controls.CONTROL_LIST);
      GUIControl.HideControl(GetID,(int)Controls.CONTROL_THUMBS);
      GUIControl.HideControl(GetID,(int)Controls.CONTROL_ALBUMS);
      
      int iControl=(int)Controls.CONTROL_LIST;
      if (ViewByIcon)
      {
        if (m_Mode==Mode.ShowAlbums)
          iControl=(int)Controls.CONTROL_ALBUMS;
        else
          iControl=(int)Controls.CONTROL_THUMBS;
      }
      GUIControl.ShowControl(GetID,iControl);
      GUIControl.FocusControl(GetID,iControl);

      string strLine="";
      View view=currentView;
      if (m_Mode==Mode.ShowArtists) view=currentViewRoot;
      switch (view)
      {
        case View.VIEW_AS_LIST:
          strLine=GUILocalizeStrings.Get(101);
          break;
        case View.VIEW_AS_ICONS:
          strLine=GUILocalizeStrings.Get(100);
          break;
        case View.VIEW_AS_LARGEICONS:
          strLine=GUILocalizeStrings.Get(417);
          break;
      }
      GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_BTNVIEWASICONS,strLine);

      SortMethod sortmethod=currentSortMethod;
      if (m_Mode==Mode.ShowArtists)
        sortmethod=currentSortMethodRoot;
      switch (sortmethod)
      {
        case SortMethod.SORT_NAME:
          strLine=GUILocalizeStrings.Get(103);
          break;
        case SortMethod.SORT_DATE:
          strLine=GUILocalizeStrings.Get(104);
          break;
        case SortMethod.SORT_SIZE:
          strLine=GUILocalizeStrings.Get(105);
          break;
        case SortMethod.SORT_TRACK:
          strLine=GUILocalizeStrings.Get(266);
          break;
        case SortMethod.SORT_DURATION:
          strLine=GUILocalizeStrings.Get(267);
          break;
        case SortMethod.SORT_TITLE:
          strLine=GUILocalizeStrings.Get(268);
          break;
        case SortMethod.SORT_ARTIST:
          strLine=GUILocalizeStrings.Get(269);
          break;
        case SortMethod.SORT_ALBUM:
          strLine=GUILocalizeStrings.Get(270);
          break;
        case SortMethod.SORT_FILENAME:
          strLine=GUILocalizeStrings.Get(363);
					break;
				case SortMethod.SORT_RATING:
					strLine=GUILocalizeStrings.Get(367);
					break;
      }
      GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_BTNSORTBY,strLine);

      bool bAsc=m_bSortAscending;
      if (m_Mode==Mode.ShowArtists)
        bAsc=m_bSortAscendingRoot;
      if (bAsc)
        GUIControl.DeSelectControl(GetID,(int)Controls.CONTROL_BTNSORTASC);
      else
        GUIControl.SelectControl(GetID,(int)Controls.CONTROL_BTNSORTASC);

      GUIControl.EnableControl(GetID,(int)Controls.CONTROL_BTNSORTBY);
      GUIControl.EnableControl(GetID,(int)Controls.CONTROL_BTNSORTASC);
    }
	  void keyboard_TextChanged(int kindOfSearch,string data)
	  {
		  DisplayArtistsList(kindOfSearch,data);
	  }
    void ShowThumbPanel()
    {
      int iItem=GetSelectedItemNo(); 
      if (m_Mode!=Mode.ShowAlbums)
      {
        if ( ViewByLargeIcon )
        {
          GUIThumbnailPanel pControl=(GUIThumbnailPanel)GetControl((int)Controls.CONTROL_THUMBS);
          pControl.ShowBigIcons(true);
        }
        else
        {
          GUIThumbnailPanel pControl=(GUIThumbnailPanel)GetControl((int)Controls.CONTROL_THUMBS);
          pControl.ShowBigIcons(false);
        }
      }
      if (iItem>-1)
      {
        GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_LIST,iItem);
        GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_THUMBS,iItem);
        GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_ALBUMS,iItem);
      }
      UpdateButtons();
    }

    void OnRetrieveCoverArt(GUIListItem item)
    {
      if (m_Mode==Mode.ShowArtists)
      {
        Utils.SetDefaultIcons(item);
        string strThumb=GUIMusicArtists.GetCoverArt(item.Label); // get artist cover art
        if (strThumb!=String.Empty )
        {
          item.IconImageBig=strThumb;
          item.IconImage=strThumb;
          item.ThumbnailImage=strThumb;
        }
      }
      else if (m_Mode==Mode.ShowAlbums)
      {
        item.ThumbnailImage="defaultAlbum.png";
        item.IconImageBig="defaultAlbumBig.png";
        item.IconImage="defaultAlbum.png";

        MusicTag tag=item.MusicTag as MusicTag;
        
        string thumb=GUIMusicFiles.GetAlbumThumbName(tag.Artist,tag.Album);
        if (System.IO.File.Exists(thumb))
        {
          item.ThumbnailImage=thumb;
          item.IconImageBig=thumb;
          item.IconImage=thumb;
        }
        else
        {
          ArrayList songstmp=new ArrayList();
          m_database.GetSongsByAlbum(tag.Album,ref songstmp);
          foreach (Song song in songstmp)
          {
            thumb = Utils.GetFolderThumb(song.FileName);
            if (System.IO.File.Exists(thumb))
            {
              item.ThumbnailImage=thumb;
              item.IconImageBig=thumb;
              item.IconImage=thumb;
              break;
            }
          }
        }
      }
      else if (m_Mode==Mode.ShowSongs)
      {
        // get thumbs...
        Utils.SetDefaultIcons(item);
        MusicTag tag = (MusicTag)item.MusicTag;
        string strThumb=GUIMusicFiles.GetCoverArt(item.IsFolder,item.Path,tag);
        if (strThumb!=string.Empty)
        {
          item.ThumbnailImage=strThumb;
          item.IconImageBig=strThumb;
          item.IconImage=strThumb;
        }
      }
      if (ViewByIcon &&!ViewByLargeIcon) 
        item.IconImage=item.IconImageBig;
    }

    void LoadDirectory(string strNewDirectory, Mode newMode)
    {
      Mode oldMode=m_Mode;
      GUIListItem SelectedItem = GetSelectedItem();
      if (SelectedItem!=null) 
      {
        if (SelectedItem.IsFolder && SelectedItem.Label!="..")
        {
          m_history.Set(SelectedItem.Label, m_strDirectory);
        }
      }
      m_Mode=newMode;
      if (m_Mode==Mode.ShowArtists)
      {
        m_strAlbum="";
        m_strArtist="";
      }
      if (m_Mode==Mode.ShowAlbums)
      {
        m_strAlbum="";
      }
      m_strDirectory=m_strArtist+m_strAlbum;
      GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST);
      GUIControl.ClearControl(GetID,(int)Controls.CONTROL_THUMBS);
      GUIControl.ClearControl(GetID,(int)Controls.CONTROL_ALBUMS);
            
      string strObjects="";
      string strNavigation="";

      ArrayList itemlist=new ArrayList();
      if (m_Mode==Mode.ShowArtists)
      {
        strNavigation=GUILocalizeStrings.Get(133);
        ArrayList artists=new ArrayList();
        m_database.GetArtists(ref artists);
        foreach(string strArtist in artists)
        {
          GUIListItem item=new GUIListItem();
          item.Label=strArtist;
          item.Path=strArtist;
          item.IsFolder=true;
          item.OnRetrieveArt +=new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
          itemlist.Add(item);
        }
      } //if (m_Mode==Mode.ShowArtists)
      else if (m_Mode==Mode.ShowAlbums)
      {
        strNavigation=GUILocalizeStrings.Get(639)+" "+m_strArtist;
        GUIListItem pItem = new GUIListItem ("..");
        pItem.Path="";
        pItem.IsFolder=true;
        Utils.SetDefaultIcons(pItem);
        if (ViewByIcon &&!ViewByLargeIcon) 
          pItem.IconImage=pItem.IconImageBig;
        itemlist.Add(pItem);
      
        ArrayList Albums = new ArrayList();
        m_database.GetAlbums(ref Albums);

        foreach(AlbumInfo info in Albums)
        {
          if ( String.Compare(info.Artist,m_strArtist,true)==0)
          {
            if(!info.Album.Equals("unknown"))
            {
              GUIListItem item=new GUIListItem();
              item.Label=info.Album;
              item.Label2=info.Artist;
              item.Path=info.Album;
              item.IsFolder=true;
              MusicTag tag=new MusicTag();
              tag.Title=" ";
              tag.Genre=info.Genre;
              tag.Year=info.Year;
              tag.Album=info.Album;
              tag.Artist=info.Artist;
              item.MusicTag=tag;
              item.OnRetrieveArt +=new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
              
              //Utils.SetDefaultIcons(item);
              itemlist.Add(item);
						}
          }
        }
        // Show the songs for the unknown albums rather than have them in
        // an unknown folder!
        ArrayList songs=new ArrayList();
        m_database.GetSongsByArtist(m_strArtist,ref songs);
        foreach (Song song in songs)
        {
          if (( m_strAlbum.Length==0||String.Compare(song.Album,m_strAlbum,true)==0) &&
              song.Album.Equals("unknown"))
          {
            GUIListItem item=new GUIListItem();
            item.Label=song.Title;
            item.IsFolder=false;
            item.Path=song.FileName;
            item.Duration=song.Duration;

            MusicTag tag=new MusicTag();
            tag.Title=song.Title;
            tag.Album=song.Album;
            tag.Artist=song.Artist;
            tag.Duration=song.Duration;
            tag.Genre=song.Genre;
            tag.Track=song.Track;
						tag.Year=song.Year;
						tag.Rating=song.Rating;
            item.MusicTag=tag;
            Utils.SetDefaultIcons(item);

            itemlist.Add(item);
          }
        }

        if (itemlist.Count<=1)
        {
          if (oldMode==Mode.ShowArtists)
            LoadDirectory(m_strDirectory, Mode.ShowSongs);
          else
            LoadDirectory(m_strDirectory, Mode.ShowArtists);
          return;
        }
      } // else if (m_Mode==Mode.ShowAlbums)
      else if (m_Mode==Mode.ShowSongs)
      { 
        strNavigation=m_strArtist+ "/" + m_strAlbum;
        GUIListItem pItem = new GUIListItem ("..");
        pItem.Path="";
        pItem.IsFolder=true;
        Utils.SetDefaultIcons(pItem);
        if (ViewByIcon &&!ViewByLargeIcon) 
          pItem.IconImage=pItem.IconImageBig;
        itemlist.Add(pItem);

        ArrayList songs=new ArrayList();
        m_database.GetSongsByArtist(m_strArtist,ref songs);
        foreach (Song song in songs)
        {
          if ( m_strAlbum.Length==0||String.Compare(song.Album,m_strAlbum,true)==0)
          {
            GUIListItem item=new GUIListItem();
            item.Label=song.Title;
            item.IsFolder=false;
            item.Path=song.FileName;
            item.Duration=song.Duration;
            
            MusicTag tag=new MusicTag();
            tag.Title=song.Title;
            tag.Album=song.Album;
            tag.Artist=song.Artist;
            tag.Duration=song.Duration;
            tag.Genre=song.Genre;
            tag.Track=song.Track;
						tag.Year=song.Year;
						tag.Rating=song.Rating;
            item.MusicTag=tag;
            item.OnRetrieveArt +=new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);

      
            itemlist.Add(item);
          }
        }
      } // of else if (m_Mode==Mode.ShowSongs)

     
      string strSelectedItem=m_history.Get(m_strDirectory); 
      int iItem=0;
      foreach (GUIListItem item in itemlist)
      {
        GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST,item);
        GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_THUMBS,item);
        GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_ALBUMS,item);
      }
      OnSort();
      int iTotalItems=itemlist.Count;
      if (itemlist.Count>0)
      {
        GUIListItem rootItem=(GUIListItem)itemlist[0];
        if (rootItem.Label=="..") iTotalItems--;
      }
      strObjects=String.Format("{0} {1}", iTotalItems, GUILocalizeStrings.Get(632));
			GUIPropertyManager.SetProperty("#itemcount",strObjects);

      GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_LABELFILES,strObjects);
      GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_LABEL,strNavigation);
      
      SetLabels();
      ShowThumbPanel();
      OnSort();
      for (int i=0; i< GetItemCount();++i)
      {
        GUIListItem item =GetItem(i);
        if (item.Label==strSelectedItem)
        {
          GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_LIST,iItem);
          GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_THUMBS,iItem);
          GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_ALBUMS,iItem);
          break;
        }
        iItem++;
      }
      if (m_iItemSelected>=0)
      {
        GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_LIST,m_iItemSelected);
        GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_THUMBS,m_iItemSelected);
        GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_ALBUMS,m_iItemSelected);
		  }
	  }

	  //
	  void DisplayArtistsList(int searchKind,string strSearchText)
	  {
			  
		Mode oldMode=m_Mode;
		m_Mode=Mode.ShowArtists;

		m_strAlbum="";
		m_strArtist="";

		GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST);
		GUIControl.ClearControl(GetID,(int)Controls.CONTROL_THUMBS);
		GUIControl.ClearControl(GetID,(int)Controls.CONTROL_ALBUMS);
            
		string strObjects="";
		string strNavigation="";

		ArrayList itemlist=new ArrayList();
		strNavigation=GUILocalizeStrings.Get(133);
		ArrayList artists=new ArrayList();

		m_database.GetArtists(searchKind,strSearchText,ref artists);

		foreach(string strArtist in artists)
			{
				GUIListItem item=new GUIListItem();
				item.Label=strArtist;
				item.Path=strArtist;
				item.IsFolder=true;
				Utils.SetDefaultIcons(item);
				string strThumb=GUIMusicArtists.GetCoverArt(item.Label); // artist cover art
				if (strThumb!=String.Empty )
				{
					item.IconImageBig=strThumb;
					item.IconImage=strThumb;
					item.ThumbnailImage=strThumb;
				}
				itemlist.Add(item);
			}
			 // get thumbs
		  //
		  m_history.Set(m_strDirectory, m_strDirectory); //save where we are
		  GUIListItem dirUp=new GUIListItem("..");
		  dirUp.Path=m_strDirectory; // to get where we are
		  dirUp.IsFolder=true;
		  Utils.SetDefaultIcons(dirUp);
      if (ViewByIcon &&!ViewByLargeIcon) 
        dirUp.IconImage=dirUp.IconImageBig;
		  itemlist.Insert(0,dirUp);
		  //

			  foreach (GUIListItem item in itemlist)
			  {
				  GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST,item);
				  GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_THUMBS,item);
				  GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_ALBUMS,item);
			  }
			  OnSort();
			  int iTotalItems=itemlist.Count;
			  if (itemlist.Count>0)
			  {
				  GUIListItem rootItem=(GUIListItem)itemlist[0];
				  if (rootItem.Label=="..") iTotalItems--;
			  }
			  strObjects=String.Format("{0} {1}", iTotalItems, GUILocalizeStrings.Get(632));
			  GUIPropertyManager.SetProperty("#itemcount",strObjects);
			  GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_LABELFILES,strObjects);
			  GUIControl.SetControlLabel(GetID,(int)Controls.CONTROL_LABEL,strNavigation);
      
			  SetLabels();
			  ShowThumbPanel();
			  OnSort();
		  
	  }
	  #endregion

    void SetLabels()
    {
      SortMethod method=currentSortMethod;
      bool bAscending=m_bSortAscending;
      if (m_Mode==Mode.ShowArtists)
      {
        method=currentSortMethodRoot;
        bAscending=m_bSortAscendingRoot;
      }

      for (int i=0; i < GetItemCount();++i)
      {
        GUIListItem item=GetItem(i);
        MusicTag tag=(MusicTag)item.MusicTag;
        if (tag!=null)
        {
          if (tag.Title.Length>0)
          {
            if (tag.Artist.Length>0)
            {
              if (tag.Track>0)
                item.Label=String.Format("{0:00}. {1} - {2}",tag.Track, tag.Artist, tag.Title);
              else
                item.Label=String.Format("{0} - {1}",tag.Artist, tag.Title);
            }
            else
            {
              if (tag.Track>0)
                item.Label=String.Format("{0:00}. {1} ",tag.Track, tag.Title);
              else
                item.Label=String.Format("{0}",tag.Title);
            }
            if (method==SortMethod.SORT_ALBUM)
            {
              if (tag.Album.Length>0 && tag.Title.Length>0)
              {
                item.Label=String.Format("{0} - {1}", tag.Album,tag.Title);
              }
						}
						if (method==SortMethod.SORT_RATING)
						{
							item.Label2=String.Format("{0}", tag.Rating);
						}
          }
        }
        
        
        if (method==SortMethod.SORT_SIZE||method==SortMethod.SORT_FILENAME)
        {
          if (item.IsFolder) item.Label2="";
          else
          {
            if (item.Size>0)
            {
              item.Label2=Utils.GetSize( item.Size);
            }
            if (method==SortMethod.SORT_FILENAME)
            {
              item.Label=Utils.GetFilename(item.Path);
            }
          }
        }
        else if (method==SortMethod.SORT_DATE)
        {
          if (item.FileInfo!=null)
          {
            item.Label2 =item.FileInfo.CreationTime.ToShortDateString() + " "+item.FileInfo.CreationTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
          }
        }
        else if (method != SortMethod.SORT_RATING)
        {
          if (tag!=null)
          {
            int nDuration=tag.Duration;
            if (nDuration>0)
            {
              item.Label2=Utils.SecondsToHMSString(nDuration);
            }
          }
        }
      }
    }

    void OnClick(int iItem)
    {
      GUIListItem item = GetSelectedItem();
      if (item==null) return;
      m_iItemSelected=-1;
      if (m_Mode==Mode.ShowArtists)
      {
        m_strArtist=item.Label;
        LoadDirectory(m_strDirectory,Mode.ShowAlbums);
      }
      else if (m_Mode==Mode.ShowAlbums && (item.MusicTag == null ||
              ((MediaPortal.TagReader.MusicTag)item.MusicTag).Album != "unknown"))
      {
        if ( item.Label=="..")
        {
          m_strArtist="";
          LoadDirectory(m_strDirectory,Mode.ShowArtists);
        }
        else
        {
          m_strAlbum=item.Label;
          LoadDirectory(m_strDirectory,Mode.ShowSongs);
        }
      }
      else
      {
        if ( item.Label=="..")
        {
          m_strAlbum="";
          LoadDirectory(m_strDirectory,Mode.ShowAlbums);
          return;
        }

        
        // play item
        //play and add current directory to temporary playlist
        int nFolderCount=0;
        PlayListPlayer.GetPlaylist( PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP ).Clear();
        PlayListPlayer.Reset();
        for ( int i = 0; i < (int) GetItemCount(); i++ ) 
        {
          GUIListItem pItem=GetItem(i);
          if ( pItem.IsFolder ) 
          {
            nFolderCount++;
            continue;
          }
          PlayList.PlayListItem playlistItem = new Playlists.PlayList.PlayListItem();
          playlistItem.Type = Playlists.PlayList.PlayListItem.PlayListItemType.Audio;
          playlistItem.FileName=pItem.Path;
          playlistItem.Description=pItem.Label;
          int iDuration=0;
          MusicTag tag=(MusicTag)pItem.MusicTag;
          if (tag!=null) iDuration=tag.Duration;
          playlistItem.Duration=iDuration;
          PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP).Add(playlistItem);
        }


        //  Save current window and directory to know where the selected item was
        MusicState.TempPlaylistWindow=GetID;
        MusicState.TempPlaylistDirectory=m_strDirectory;

        PlayListPlayer.CurrentPlaylist=PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP;
        PlayListPlayer.Play(iItem-nFolderCount);
      }
    }
    void OnQueueAlbum(string strAlbum)
    {
      if (strAlbum=="" || strAlbum=="..") return;
      ArrayList songs=new ArrayList();
      m_database.GetSongsByArtist(m_strArtist,ref songs);
      foreach (Song song in songs)
      {
        if ( String.Compare(song.Album,strAlbum,true)==0)
        {
          PlayList.PlayListItem playlistItem =new PlayList.PlayListItem();
          playlistItem.Type=PlayList.PlayListItem.PlayListItemType.Audio;
          playlistItem.FileName=song.FileName;
          playlistItem.Description=song.Title;
          playlistItem.Duration=song.Duration;
          PlayListPlayer.GetPlaylist( PlayListPlayer.PlayListType.PLAYLIST_MUSIC ).Add(playlistItem);
        }
      }
      if (!g_Player.Playing)
      {
        PlayListPlayer.CurrentPlaylist =PlayListPlayer.PlayListType.PLAYLIST_MUSIC;
        PlayListPlayer.Play(0);
      }
    }

    void OnQueueArtist(string strArtist)
    {
      if (strArtist==null) return;
      if (strArtist=="") return;
      if (strArtist=="..") return;
      ArrayList albums=new ArrayList();
      m_database.GetSongsByArtist(strArtist, ref albums);
      foreach (Song song in albums)
      {
        PlayList.PlayListItem playlistItem =new PlayList.PlayListItem();
        playlistItem.Type=PlayList.PlayListItem.PlayListItemType.Audio;
        playlistItem.FileName=song.FileName;
        playlistItem.Description=song.Title;
        playlistItem.Duration=song.Duration;
        PlayListPlayer.GetPlaylist( PlayListPlayer.PlayListType.PLAYLIST_MUSIC ).Add(playlistItem);
      }
      
      if (!g_Player.Playing)
      {
        PlayListPlayer.CurrentPlaylist =PlayListPlayer.PlayListType.PLAYLIST_MUSIC;
        PlayListPlayer.Play(0);
      }
    }

    void OnQueueItem(int iItem)
    {
      // add item 2 playlist
      GUIListItem pItem=GetItem(iItem);
      if (m_Mode==Mode.ShowArtists)
      {
        string strArtist=pItem.Label;
        OnQueueArtist(strArtist);
      }
      else if (m_Mode==Mode.ShowAlbums)
      {
        string strAlbum=pItem.Label;
        OnQueueAlbum(strAlbum);
      }
      else if (m_Mode==Mode.ShowSongs)
      {
        AddItemToPlayList(pItem);
      }
      //move to next item
      GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_LIST,iItem+1);
      GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_THUMBS,iItem+1);
      GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_ALBUMS,iItem+1);
    }

    void AddItemToPlayList(GUIListItem pItem) 
    {
      if (pItem.IsFolder)
      {
        // recursive
        if (pItem.Label == "..") return;
        string strDirectory=m_strDirectory;
        m_strDirectory=pItem.Path;
        
        ArrayList itemlist=m_directory.GetDirectory(m_strDirectory);
        foreach (GUIListItem item in itemlist)
        {
          AddItemToPlayList(item);
        }
      }
      else
      {
        //TODO
        if (Utils.IsAudio(pItem.Path) && !PlayListFactory.IsPlayList(pItem.Path))
        {
          PlayList.PlayListItem playlistItem =new PlayList.PlayListItem();
          playlistItem.Type=PlayList.PlayListItem.PlayListItemType.Audio;
          playlistItem.FileName=pItem.Path;
          playlistItem.Description=pItem.Label;
          playlistItem.Duration=pItem.Duration;
          PlayListPlayer.GetPlaylist( PlayListPlayer.PlayListType.PLAYLIST_MUSIC ).Add(playlistItem);
        }
      }
    }

    void LoadPlayList(string strPlayList)
    {
      PlayList playlist=PlayListFactory.Create(strPlayList);
      if (playlist==null) return;
      if (!playlist.Load(strPlayList))
      {
        GUIDialogOK dlgOK=(GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
        if (dlgOK!=null)
        {
          dlgOK.SetHeading(6);
          dlgOK.SetLine(1,477);
          dlgOK.SetLine(2,"");
          dlgOK.DoModal(GetID);
        }
        return;
      }

      if (playlist.Count==1)
			{
				Log.Write("GUIMusicArtists:Play:{0}",playlist[0].FileName);
        g_Player.Play(playlist[0].FileName);
        return;
      }

      // clear current playlist
      PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC).Clear();

      // add each item of the playlist to the playlistplayer
      for (int i=0; i < playlist.Count; ++i)
      {
        PlayList.PlayListItem playListItem =playlist[i];
        PlayListPlayer.GetPlaylist( PlayListPlayer.PlayListType.PLAYLIST_MUSIC ).Add(playListItem);
      }

      
      // if we got a playlist
      if (PlayListPlayer.GetPlaylist( PlayListPlayer.PlayListType.PLAYLIST_MUSIC ).Count >0)
      {
        // then get 1st song
        playlist=PlayListPlayer.GetPlaylist( PlayListPlayer.PlayListType.PLAYLIST_MUSIC );
        PlayList.PlayListItem item=playlist[0];

        // and start playing it
        PlayListPlayer.CurrentPlaylist=PlayListPlayer.PlayListType.PLAYLIST_MUSIC;
        PlayListPlayer.Reset();
        PlayListPlayer.Play(0);

        // and activate the playlist window if its not activated yet
        if (GetID == GUIWindowManager.ActiveWindow)
        {
          GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST);
        }
      }
    }


    #region Sort Members
    void OnSort()
    {
      SortMethod method=currentSortMethod;
      SetLabels();
      GUIListControl list=(GUIListControl)GetControl((int)Controls.CONTROL_LIST);
      list.Sort(this);
      GUIThumbnailPanel panel=(GUIThumbnailPanel)GetControl((int)Controls.CONTROL_THUMBS);
      panel.Sort(this);
      list=(GUIListControl)GetControl((int)Controls.CONTROL_ALBUMS);
      list.Sort(this);

      UpdateButtons();
    }

    public int Compare(object x, object y)
    {
      if (x==y) return 0;
      GUIListItem item1=(GUIListItem)x;
      GUIListItem item2=(GUIListItem)y;
      if (item1==null) return -1;
      if (item2==null) return -1;
      if (item1.IsFolder && item1.Label=="..") return -1;
      if (item2.IsFolder && item2.Label=="..") return -1;
      if (item1.IsFolder && !item2.IsFolder) return -1;
      else if (!item1.IsFolder && item2.IsFolder) return 1; 

      string strSize1="";
      string strSize2="";
      if (item1.FileInfo!=null) strSize1=Utils.GetSize(item1.FileInfo.Length);
      if (item2.FileInfo!=null) strSize2=Utils.GetSize(item2.FileInfo.Length);

      SortMethod method=currentSortMethod;
      bool bAscending=m_bSortAscending;
      if (m_Mode==Mode.ShowArtists)
      {
        method=currentSortMethodRoot;
        bAscending=m_bSortAscendingRoot;
      }
      switch (method)
      {
        case SortMethod.SORT_NAME:
          if (bAscending)
          {
            return String.Compare(item1.Label ,item2.Label,true);
          }
          else
          {
            return String.Compare(item2.Label ,item1.Label,true);
          }
        

        case SortMethod.SORT_DATE:
          if (item1.FileInfo==null) return -1;
          if (item2.FileInfo==null) return -1;
          
          item1.Label2 =item1.FileInfo.CreationTime.ToShortDateString() + " "+item1.FileInfo.CreationTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
          item2.Label2 =item2.FileInfo.CreationTime.ToShortDateString() + " "+item2.FileInfo.CreationTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
          if (bAscending)
          {
            return DateTime.Compare(item1.FileInfo.CreationTime,item2.FileInfo.CreationTime);
          }
          else
          {
            return DateTime.Compare(item2.FileInfo.CreationTime,item1.FileInfo.CreationTime);
          }

				case SortMethod.SORT_RATING:
					int iRating1 = 0;
					int iRating2 = 0;
					if (item1.MusicTag != null) iRating1 = ((MusicTag)item1.MusicTag).Rating;
					if (item2.MusicTag != null) iRating2 = ((MusicTag)item2.MusicTag).Rating;
					if (bAscending)
					{
						return (int)(iRating1 - iRating2);
					}
					else
					{
						return (int)(iRating2 - iRating1);
					}

        case SortMethod.SORT_SIZE:
          if (item1.FileInfo==null) return -1;
          if (item2.FileInfo==null) return -1;
          if (bAscending)
          {
            return (int)(item1.FileInfo.Length - item2.FileInfo.Length);
          }
          else
          {
            return (int)(item2.FileInfo.Length - item1.FileInfo.Length);
          }

        case SortMethod.SORT_TRACK:
          int iTrack1=0;
          int iTrack2=0;
          if (item1.MusicTag!=null) iTrack1=((MusicTag)item1.MusicTag).Track;
          if (item2.MusicTag!=null) iTrack2=((MusicTag)item2.MusicTag).Track;
          if (bAscending)
          {
            return (int)(iTrack1 - iTrack2);
          }
          else
          {
            return (int)(iTrack2 - iTrack1);
          }
          
        case SortMethod.SORT_DURATION:
          int iDuration1=0;
          int iDuration2=0;
          if (item1.MusicTag!=null) iDuration1=((MusicTag)item1.MusicTag).Duration;
          if (item2.MusicTag!=null) iDuration2=((MusicTag)item2.MusicTag).Duration;
          if (bAscending)
          {
            return (int)(iDuration1 - iDuration2);
          }
          else
          {
            return (int)(iDuration2 - iDuration1);
          }
          
        case SortMethod.SORT_TITLE:
          string strTitle1=item1.Label;
          string strTitle2=item2.Label;
          if (item1.MusicTag!=null) strTitle1=((MusicTag)item1.MusicTag).Title;
          if (item2.MusicTag!=null) strTitle2=((MusicTag)item2.MusicTag).Title;
          if (bAscending)
          {
            return String.Compare(strTitle1 ,strTitle2,true);
          }
          else
          {
            return String.Compare(strTitle2 ,strTitle1,true);
          }
        
        case SortMethod.SORT_ARTIST:
          string strArtist1="";
          string strArtist2="";
          if (item1.MusicTag!=null) strArtist1=((MusicTag)item1.MusicTag).Artist;
          if (item2.MusicTag!=null) strArtist2=((MusicTag)item2.MusicTag).Artist;
          if (bAscending)
          {
            return String.Compare(strArtist1 ,strArtist2,true);
          }
          else
          {
            return String.Compare(strArtist2 ,strArtist1,true);
          }
        
        case SortMethod.SORT_ALBUM:
          string strAlbum1="";
          string strAlbum2="";
          if (item1.MusicTag!=null) strAlbum1=((MusicTag)item1.MusicTag).Album;
          if (item2.MusicTag!=null) strAlbum2=((MusicTag)item2.MusicTag).Album;
          if (bAscending)
          {
            return String.Compare(strAlbum1 ,strAlbum2,true);
          }
          else
          {
            return String.Compare(strAlbum2 ,strAlbum1,true);
          }
          

        case SortMethod.SORT_FILENAME:
          string strFile1=System.IO.Path.GetFileName(item1.Path);
          string strFile2=System.IO.Path.GetFileName(item2.Path);
          if (bAscending)
          {
            return String.Compare(strFile1 ,strFile2,true);
          }
          else
          {
            return String.Compare(strFile2 ,strFile1,true);
          }
          
      } 
      return 0;
    }
    #endregion

    void OnInfo(int iItem)
    {
      m_iItemSelected=GetSelectedItemNo();
      if (m_Mode==Mode.ShowAlbums)
      {
        OnInfoAlbum(iItem);
        return;
      }
      else if (m_Mode!=Mode.ShowArtists) return;
      GUIListItem item = GetSelectedItem();
      if (item==null) return;
      if (item.Label=="") return;
      
      int iSelectedItem=GetSelectedItemNo();
      GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      GUIDialogProgress dlgProgress=(GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      GUIListItem pItem=GetItem(iItem);

      string strPath="";
      if (pItem.IsFolder)
        strPath=pItem.Path;
      else
      {
        string strFileName;
        DatabaseUtility.Split(pItem.Path, out strPath, out strFileName);
      }

      //	Try to find an album name for this item.
      //	Only save to database, if album name is found there.
      bool bSaveDb=true;
      string strAlbumName=pItem.Label;
      string strArtistName=strAlbumName;
      /*
      MusicTag tag=(MusicTag)(pItem.MusicTag);
      if (tag!=null)
      {
        strArtistName = tag.Artist;
        if (tag.Album.Length>0) 
        {
          strAlbumName=tag.Album;
          bSaveDb=true;
        }
        else if (tag.Title.Length>0) 
        {
          strAlbumName=tag.Title;
          bSaveDb=true;
        }
      }
      */

      MusicAlbumInfo infoTag = (MusicAlbumInfo)pItem.AlbumInfoTag;
      if (infoTag!=null)
      {
        //	Handle files
        AlbumInfo album=new AlbumInfo();
        string strAlbum=infoTag.Title;
        //	Is album in database?
        /* TODO
        if (m_database.GetAlbumByPath(strPath, ref album))
        {
          //	yes, save query results to database
          strAlbumName=album.Album;
          bSaveDb=true;
        }
        else
          //	no, don't save
          strAlbumName=strAlbum;*/
      }

      // check cache
      ArtistInfo artistinfo=new ArtistInfo();
      if ( m_database.GetArtistInfo(strArtistName, strPath, ref artistinfo) )
      {
        ArrayList songs=new ArrayList();
        MusicArtistInfo artist = new MusicArtistInfo();
        artist.Set(artistinfo);

        // ok, show Artist info
        GUIMusicArtistInfo pDlgArtistInfo= (GUIMusicArtistInfo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_ARTIST_INFO);
        if (null!=pDlgArtistInfo)
        {
          pDlgArtistInfo.Artist=artist;
          pDlgArtistInfo.DoModal(GetID);
                
          if (pDlgArtistInfo.NeedsRefresh)
          {
            m_database.DeleteArtistInfo(artist.Artist);
            OnInfo(iItem);
            return;
          }
        }
        return;
      }

      
      if (null!=pDlgOK && !Util.Win32API.IsConnectedToInternet())
      {
        pDlgOK.SetHeading(703);
        pDlgOK.SetLine(1,703);
        pDlgOK.SetLine(2,"");
        pDlgOK.DoModal(GetID);
        return;
      }
      else if(!Util.Win32API.IsConnectedToInternet())
      {
        return;
      }

      // show dialog box indicating we're searching the artist
      if (dlgProgress!=null)
      {
        dlgProgress.SetHeading(320);
        dlgProgress.SetLine(1,strArtistName);
        dlgProgress.SetLine(2,"");
        dlgProgress.StartModal(GetID);
        dlgProgress.Progress();
      }
      bool bDisplayErr=false;

      // find artist info
      AllmusicSiteScraper scraper = new AllmusicSiteScraper();
      if (scraper.FindInfo(AllmusicSiteScraper.SearchBy.Artists, strArtistName))
      {
        if (dlgProgress!=null) dlgProgress.Close();
        // did we found at least 1 album?
        if (scraper.IsMultiple())
        {
          //yes
          // if we found more then 1 album, let user choose one
          int iSelectedAlbum=0;
          string[] artistsFound = scraper.GetItemsFound();
          //show dialog with all albums found
          string szText=GUILocalizeStrings.Get(181);
          GUIDialogSelect pDlg= (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
          if (null!=pDlg)
          {
            pDlg.Reset();
            pDlg.SetHeading(szText);
            for (int i=0; i < artistsFound.Length; ++i)
            {
              pDlg.Add(artistsFound[i]);
            }
            pDlg.DoModal(GetID);

            // and wait till user selects one
            iSelectedAlbum= pDlg.SelectedLabel;
            if (iSelectedAlbum< 0) return;
          }

          // ok, now show dialog we're downloading the artist info
          if (null!=dlgProgress) 
          {
            dlgProgress.SetHeading(320);
            dlgProgress.SetLine(1,strArtistName);
            dlgProgress.SetLine(2,"");
            dlgProgress.StartModal(GetID);
            dlgProgress.Progress();
          }

          // download the artist info
          if(scraper.FindInfoByIndex(iSelectedAlbum))
          {
            if (null!=dlgProgress) 
              dlgProgress.Close();
            MusicArtistInfo artistInfo = new MusicArtistInfo();
            if(artistInfo.Parse(scraper.GetHtmlContent()))
            {
              // if the artist selected from allmusic.com does not match
              // the one from the file, override the one from the allmusic
              // with the one from the file so the info is correct in the
              // database...
              if(!artistInfo.Artist.Equals(strArtistName))
                artistInfo.Artist = strArtistName;

              if (bSaveDb)
              {
                m_database.AddArtistInfo(artistInfo.Get());
              }

              // ok, show Artist info
              GUIMusicArtistInfo pDlgArtistInfo= (GUIMusicArtistInfo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_ARTIST_INFO);
              if (null!=pDlgArtistInfo)
              {
                pDlgArtistInfo.Artist=artistInfo;
                pDlgArtistInfo.DoModal(GetID);
                
                if (pDlgArtistInfo.NeedsRefresh)
                {
                  m_database.DeleteArtistInfo(artistInfo.Artist);
                  OnInfo(iItem);
                  return;
                }
              }
            }
          }

          if (null!=dlgProgress) 
            dlgProgress.Close();
        }
        else // single
        {
          MusicArtistInfo artistInfo = new MusicArtistInfo();
          if(artistInfo.Parse(scraper.GetHtmlContent()))
          {
            // if the artist selected from allmusic.com does not match
            // the one from the file, override the one from the allmusic
            // with the one from the file so the info is correct in the
            // database...
            if(!artistInfo.Artist.Equals(strArtistName))
              artistInfo.Artist = strArtistName;

            if (bSaveDb)
            {
              // save to database
              m_database.AddArtistInfo(artistInfo.Get());
            }

            // ok, show Artist info
            GUIMusicArtistInfo pDlgArtistInfo= (GUIMusicArtistInfo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_ARTIST_INFO);
            if (null!=pDlgArtistInfo)
            {
              pDlgArtistInfo.Artist=artistInfo;
              pDlgArtistInfo.DoModal(GetID);
                
              if (pDlgArtistInfo.NeedsRefresh)
              {
                m_database.DeleteArtistInfo(artistInfo.Artist);
                OnInfo(iItem);
                return;
              }
            }
          }
        }
      }
      else
      {
        // unable 2 connect to www.allmusic.com
        bDisplayErr=true;
      }
      // if an error occured, then notice the user
      if (bDisplayErr)
      {
        if (null!=dlgProgress) 
          dlgProgress.Close();
        if (null!=pDlgOK)
        {
          pDlgOK.SetHeading(702);
          pDlgOK.SetLine(1,702);
          pDlgOK.SetLine(2,"");
          pDlgOK.DoModal(GetID);
        }
      }
    }

    void OnInfoAlbum(int iItem)
    {
      m_iItemSelected=GetSelectedItemNo();
      if (m_Mode!=Mode.ShowAlbums) return;
      GUIListItem item = GetSelectedItem();
      if (item==null) return;
      if (item.Label=="") return;
      
      int iSelectedItem=GetSelectedItemNo();
      GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      GUIDialogProgress dlgProgress=(GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      GUIListItem pItem=GetItem(iItem);

      string strPath="";
      if (pItem.IsFolder)
        strPath=pItem.Path;
      else
      {
        string strFileName;
        DatabaseUtility.Split(pItem.Path, out strPath, out strFileName);
      }

      //	Try to find an album name for this item.
      //	Only save to database, if album name is found there.
      bool bSaveDb=false;
      string strAlbumName=pItem.Label;
      MusicTag tag=(MusicTag)(pItem.MusicTag);
      if (tag!=null)
      {
        if (tag.Album.Length>0) 
        {
          strAlbumName=tag.Album;
          bSaveDb=true;
        }
        else if (tag.Title.Length>0) 
        {
          strAlbumName=tag.Title;
          bSaveDb=true;
        }
      }

      MusicAlbumInfo infoTag = (MusicAlbumInfo)pItem.AlbumInfoTag;
      if (infoTag!=null)
      {
        //	Handle files
        AlbumInfo album=new AlbumInfo();
        string strAlbum=infoTag.Title;
        //	Is album in database?
        /* TODO
        if (m_database.GetAlbumByPath(strPath, ref album))
        {
          //	yes, save query results to database
          strAlbumName=album.Album;
          bSaveDb=true;
        }
        else
          //	no, don't save
          strAlbumName=strAlbum;*/
      }


      // check cache
      AlbumInfo albuminfo=new AlbumInfo();
      if ( m_database.GetAlbumInfo(strAlbumName, strPath, ref albuminfo) )
      {
        ArrayList songs=new ArrayList();
        MusicAlbumInfo album = new MusicAlbumInfo();
        album.Set(albuminfo);

        GUIMusicInfo pDlgAlbumInfo= (GUIMusicInfo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_MUSIC_INFO);
        if (null!=pDlgAlbumInfo)
        {
          pDlgAlbumInfo.Album=album;
          pDlgAlbumInfo.Tag=tag;
          pDlgAlbumInfo.DoModal(GetID);
          
          if (pDlgAlbumInfo.NeedsRefresh)
          {
            m_database.DeleteAlbumInfo(strAlbumName);
            OnInfo(iItem);
            return;
          }
        }
        return;
      }

      // show dialog box indicating we're searching the album
      if (dlgProgress!=null)
      {
        dlgProgress.SetHeading(185);
        dlgProgress.SetLine(1,strAlbumName);
        dlgProgress.SetLine(2,"");
        dlgProgress.StartModal(GetID);
        dlgProgress.Progress();
      }
      bool bDisplayErr=false;

      // find album info
      MusicInfoScraper scraper = new MusicInfoScraper();
      if (scraper.FindAlbuminfo(strAlbumName))
      {
        if (dlgProgress!=null) dlgProgress.Close();
        // did we found at least 1 album?
        int iAlbumCount=scraper.Count;
        if (iAlbumCount >=1)
        {
          //yes
          // if we found more then 1 album, let user choose one
          int iSelectedAlbum=0;
          if (iAlbumCount > 1)
          {
            //show dialog with all albums found
            string szText=GUILocalizeStrings.Get(181);
            GUIDialogSelect pDlg= (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
            if (null!=pDlg)
            {
              pDlg.Reset();
              pDlg.SetHeading(szText);
              for (int i=0; i < iAlbumCount; ++i)
              {
                MusicAlbumInfo info = scraper[i];
                pDlg.Add(info.Title2);
              }
              pDlg.DoModal(GetID);

              // and wait till user selects one
              iSelectedAlbum= pDlg.SelectedLabel;
              if (iSelectedAlbum< 0) return;
            }
          }

          // ok, now show dialog we're downloading the album info
          MusicAlbumInfo album = scraper[iSelectedAlbum];
          if (null!=dlgProgress) 
          {
            dlgProgress.SetHeading(185);
            dlgProgress.SetLine(1,album.Title2);
            dlgProgress.SetLine(2,"");
            dlgProgress.StartModal(GetID);
            dlgProgress.Progress();
          }

          // download the album info
          bool bLoaded=album.Loaded;
          if (!bLoaded) 
            bLoaded=album.Load();
          if ( bLoaded )
          {
            // set album title from musicinfotag, not the one we got from allmusic.com
            album.Title=strAlbumName;
            // set path, needed to store album in database
            album.AlbumPath=strPath;

            if (bSaveDb)
            {
              albuminfo=new AlbumInfo();
              albuminfo.Album  = album.Title;
              albuminfo.Artist = album.Artist;
              albuminfo.Genre  = album.Genre;
              albuminfo.Tones  = album.Tones;
              albuminfo.Styles = album.Styles;
              albuminfo.Review = album.Review;
              albuminfo.Image  = album.ImageURL;
              albuminfo.Tracks=album.Tracks;
              albuminfo.Rating   = album.Rating;
              try
              {
                albuminfo.Year 		= Int32.Parse( album.DateOfRelease);
              }
              catch (Exception)
              {
              }
              //albuminfo.Path   = album.AlbumPath;
              // save to database
              m_database.AddAlbumInfo(albuminfo);



            }
            if (null!=dlgProgress) 
              dlgProgress.Close();

            // ok, show album info
            GUIMusicInfo pDlgAlbumInfo= (GUIMusicInfo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_MUSIC_INFO);
            if (null!=pDlgAlbumInfo)
            {
              pDlgAlbumInfo.Album=album;
              pDlgAlbumInfo.Tag=tag;
              pDlgAlbumInfo.DoModal(GetID);
              
              if (pDlgAlbumInfo.NeedsRefresh)
              {
                m_database.DeleteAlbumInfo(album.Title);
                OnInfo(iItem);
                return;
              }
            }
          }
          else
          {
            // failed 2 download album info
            bDisplayErr=true;
          }
        }
        else 
        {
          // no albums found
          bDisplayErr=true;
        }
      }
      else
      {
        // unable 2 connect to www.allmusic.com
        bDisplayErr=true;
      }
      // if an error occured, then notice the user
      if (bDisplayErr)
      {
        if (null!=dlgProgress) 
          dlgProgress.Close();
        if (null!=pDlgOK)
        {
          pDlgOK.SetHeading(187);
          pDlgOK.SetLine(1,187);
          pDlgOK.SetLine(2,"");
          pDlgOK.DoModal(GetID);
        }
      }
    }

    
    static public string GetCoverArtName(string artist)
    {
      return Utils.GetCoverArtName(GUIMusicFiles.ArtistsThumbsFolder, artist);
    }

    static public string GetCoverArt(string artist)
    {
      string thumb=GUIMusicArtists.GetCoverArtName(artist);
      if (System.IO.File.Exists(thumb))
      {
        return thumb;
      }
      return String.Empty;
    }
  }
}
