﻿using System;
using System.Windows;
using System.Windows.Media;
using KittyCoin.Models;
using KittyCoin.ViewModels;

namespace KittyCoin.Views
{
    /// <summary>
    /// Interaction logic for ConnectWalletView.xaml
    /// </summary>
    public partial class ConnectWalletView
    {
        public ConnectWalletViewModel ViewModel { get; }

        public ConnectWalletView(string publicAddress)
        {
            InitializeComponent();
            ViewModel = new ConnectWalletViewModel(publicAddress);
            DataContext = ViewModel;

            ViewModel.UserChanged += ChangeConnectMessage;
            ChangeConnectMessage(null, null);
        }

        private void ChangeConnectMessage(object sender, EventArgs e)
        {
            if (ViewModel.WalletConnectMessage == Constants.WALLET_CONNECTED)
            {
                ConnectMessage.Foreground = new SolidColorBrush(Color.FromArgb(255, 91, 193, 70));
            }
            else
            {
                ConnectMessage.Foreground = new SolidColorBrush(Color.FromArgb(255, 254, 77, 98));
            }
        }

        private void CopyPublicAddressToClipboard(object sender, RoutedEventArgs e)
        {
            if (ViewModel.PublicAddress != null)
            {
                try
                {
                    Clipboard.SetDataObject(ViewModel.PublicAddress);
                    MessageCopyClipboard.Text = "Copied";
                }
                catch (Exception)
                {
                    MessageCopyClipboard.Text = "Error while trying to copy the Public Address";
                }
            }
        }
    }
}
