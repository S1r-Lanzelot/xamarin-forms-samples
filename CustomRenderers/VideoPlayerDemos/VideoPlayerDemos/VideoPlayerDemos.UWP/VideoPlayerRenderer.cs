﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(MediaHelpers.VideoPlayer),
                          typeof(MediaHelpers.UWP.VideoPlayerRenderer))]

namespace MediaHelpers.UWP
{
    public class VideoPlayerRenderer : ViewRenderer<VideoPlayer, MediaElement>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<VideoPlayer> args)
        {
            base.OnElementChanged(args);

            if (Control == null)
            {
                // NOTE: MediaPlayerElement rather than MediaElement is recommended for Windows 10 Build 1607 and later,

                MediaElement mediaElement = new MediaElement();

                mediaElement.MediaOpened += (sender, e) =>
                {
                    ((IVideoPlayerController)Element).Duration = mediaElement.NaturalDuration.TimeSpan;
                };

                mediaElement.CurrentStateChanged += (sender, e) =>
                {

                };

                // MediaEnded, MediaFailed


                mediaElement.RegisterPropertyChangedCallback(MediaElement.CanPauseProperty, (x, y) =>
                {
                    ((IVideoPlayerController)Element).CanPause = mediaElement.CanPause;
                });

                mediaElement.RegisterPropertyChangedCallback(MediaElement.CanSeekProperty, (x, y) =>
                {
                    ((IVideoPlayerController)Element).CanSeek = mediaElement.CanSeek;
                });


                SetNativeControl(mediaElement);
            }

            if (args.OldElement != null)
            {
                args.OldElement.UpdateStatus -= OnUpdateStatus;

                args.OldElement.PlayRequested -= OnPlayRequested;
                args.OldElement.PauseRequested -= OnPauseRequested;
                args.OldElement.StopRequested -= OnStopRequested;
            }

            if (args.NewElement != null)
            {
                SetSource();
                SetAutoPlay();
                SetAreTransportControlsEnabled();

                args.NewElement.UpdateStatus += OnUpdateStatus;

                args.NewElement.PlayRequested += OnPlayRequested;
                args.NewElement.PauseRequested += OnPauseRequested;
                args.NewElement.StopRequested += OnStopRequested;
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(sender, args);

            if (args.PropertyName == VideoPlayer.SourceProperty.PropertyName)
            {
                SetSource();
            }
            else if (args.PropertyName == VideoPlayer.AutoPlayProperty.PropertyName)
            {
                SetAutoPlay();
            }
            else if (args.PropertyName == VideoPlayer.AreTransportControlsEnabledProperty.PropertyName)
            {
                SetAreTransportControlsEnabled();
            }
        }

        async void SetSource()
        {
            if (Element.Source != null)
            {
                if (Element.Source is UriVideoSource)
                {
                    string uriString = (Element.Source as UriVideoSource).Uri;
                    Control.Source = new Uri(uriString);
                }
                else if (Element.Source is FileVideoSource)
                {
                    // Code requires Pictures Library in Package.appxmanifest Capabilities to be enabled
                    string filename = (Element.Source as FileVideoSource).File;
                    StorageFile storageFile = await StorageFile.GetFileFromPathAsync(filename);
                    IRandomAccessStreamWithContentType stream = await storageFile.OpenReadAsync();
                    Control.SetSource(stream, storageFile.ContentType);
                }
                else if (Element.Source is ResourceVideoSource)
                {
                    string path = "ms-appx:///" + (Element.Source as ResourceVideoSource).Path;
                    Control.Source = new Uri(path);
                }
            }
        }

        void SetAutoPlay()
        {
            Control.AutoPlay = Element.AutoPlay;
        }

        void SetAreTransportControlsEnabled()
        {
            Control.AreTransportControlsEnabled = Element.AreTransportControlsEnabled;
        }

        // Event handler to update status
        void OnUpdateStatus(object sender, EventArgs args)
        {
            ((IElementController)Element).SetValueFromRenderer(VideoPlayer.PositionProperty, Control.Position);
        }

        // Event handlers to implement methods
        void OnPlayRequested(object sender, EventArgs args)
        {
            Control.Play();
        }

        void OnPauseRequested(object sender, EventArgs args)
        {
            Control.Pause();
        }

        void OnStopRequested(object sender, EventArgs args)
        {
            Control.Stop();
        }
    }
}