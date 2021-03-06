﻿using LastDitchPlayer.Classes;
using LastDitchPlayer.OrderStrategy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace LastDitchPlayer.Playlists
{
    public class Playlist : IEnumerator<Track>
    {
        public ObservableCollection<Track> Tracks { get; set; }
        public IOrderStrategy currentStrategy;
        public PlaylistSerializer currentState =>  currentStateIndx <= 0 ? new PlaylistSerializer(Tracks.listifyObservableTracks(), Name) : states[currentStateIndx];
        public List<PlaylistSerializer> states;
        public string Name;
        private int currentStateIndx = 0;



        //Custom enumerator vars
        public Track Current { get; private set; }
        object IEnumerator.Current => this.Current;
        //REMEBER: Change this variable to private when done debugging
        private int position = -1;

        public Playlist()
        {
            Tracks = new ObservableCollection<Track>();
            states = new List<PlaylistSerializer>();
        }

        #region Implement iterator interface
      
        public Track this[int index]
        {
            get { return Tracks[index]; }
            set { Tracks.Insert(index, value); }
        }

        public bool MoveNext()
        {
            Track tmp;
            tmp = currentStrategy.getNextTrack(this, ref position);
            if(tmp != null) 
            {
                Current = tmp;
                return true;
            }
            else
            {
                Current = null;
                return false;
            }
        }    
        
        public bool MovePrev()
        {
            Track tmp;
            tmp = currentStrategy.getPrevTrack(this, ref position);
            if(tmp != null) 
            {
                Current = tmp;
                return true;
            }
            else
            {
                Current = null;
                return false;
            }
        }

        public void Reset()
        {
            position = 0;
        }

        public void Dispose(){}

        #endregion

        #region Custom collection methods
        public int getLength()
        {
            return Tracks.Count;
        }

        public void addTrack(Track track)
        {
            Tracks.Add(track);
        }

        public void selectTrack(Track track)
        {
            Current = track;
            position = Tracks.IndexOf(track);
        }
        #endregion

        #region Strategy pattern
        public void setStrategy(IOrderStrategy strategy)
        {
            currentStrategy = strategy;
        }
        #endregion


        #region Memento

        /// <summary>
        /// Adds current playlist state to state list
        /// </summary>
        public void saveState()
         {
            if(currentStateIndx != 0 )
            {
                states.Clear();
                currentStateIndx = 0;
            }
            states.Insert(0,new PlaylistSerializer(Tracks.listifyObservableTracks(), this.Name));


        }

    internal void loadNextState()
        {
            currentStateIndx = currentStateIndx - 1 < 0 ? 0 : currentStateIndx - 1;
            PlaylistSerializer temp = states[currentStateIndx];
            Tracks = temp.playList.listToObservable();

        } 
        
        internal void loadPrevState()
        {
            currentStateIndx = currentStateIndx + 1 > states.Count-1 ? currentStateIndx : currentStateIndx + 1;
            PlaylistSerializer temp = states[currentStateIndx];
            Tracks = temp.playList.listToObservable();

        }

        #endregion

        /// <summary>
        /// Write playlist to XML file.
        /// </summary>
        ///  See <see cref="LastDitchPlayer.Playlists.PlaylistSerializer.Serialize(string)"/>
        public void serializePlaylist()
        {
            PlaylistSerializer state = currentState ?? new PlaylistSerializer(Tracks.listifyObservableTracks(), this.Name);
            state.Serialize(Name);
        }


    }
   
}
