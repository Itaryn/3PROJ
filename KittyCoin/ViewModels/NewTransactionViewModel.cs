﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using KittyCoin.Models;
using Prism.Commands;

namespace KittyCoin.ViewModels
{
    public class NewTransactionViewModel : INotifyPropertyChanged
    {
        private int _amount;
        private string _publicAddress;

        public EventHandler NewTransaction;

        public NewTransactionViewModel()
        {
            SendTransactionCommand = new DelegateCommand(SendTransactionMethod);
        }

        public ICommand SendTransactionCommand { get; }

        public void SendTransactionMethod()
        {
            NewTransaction.BeginInvoke(this, new EventArgsObject(new List<string> { Amount.ToString(), PublicAddress }), null, null);
        }

        #region Input

        public int Amount
        {
            get => _amount;
            set
            {
                if (_amount == value) return;
                _amount = value;
                RaisePropertyChanged("Amount");
            }
        }

        public string PublicAddress
        {
            get => _publicAddress;
            set
            {
                if (_publicAddress == value) return;
                _publicAddress = value;
                RaisePropertyChanged("PublicAddress");
            }
        }

        #endregion

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}