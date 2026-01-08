using System;



namespace EfcToXamarinAndroid.Core
{
    public class Sms
    {
        private string _id;
        private string _address;
        private string _msg;
        private string _readState;//"0" for have not read sms and "1" for have read sms
        private string _time;
        private string _folderName;

        public string Id
        {
            get => _id;
            set => _id = value;
        }
        public string Address
        {
            get => _address;
            set => _address = value;
        }
        public string Msg
        {
            get => _msg;
            set => _msg = value;
        }
        public string ReadState
        {
            get => _readState;
            set => _readState = value;
        }
        public string Time
        {
            get => _time;
            set => _time = value;
        }
        public string FolderName
        {
            get => _folderName;
            set => _folderName = value;
        }

        public String getId()
        {
            return _id;
        }
        public String getAddress()
        {
            return _address;
        }
        public String getMsg()
        {
            return _msg;
        }
        public String getReadState()
        {
            return _readState;
        }
        public String getTime()
        {
            return _time;
        }
        public String getFolderName()
        {
            return _folderName;
        }


        public void setId(String id)
        {
            _id = id;
        }
        public void setAddress(String address)
        {
            _address = address;
        }
        public void setMsg(String msg)
        {
            _msg = msg;
        }
        public void setReadState(String readState)
        {
            _readState = readState;
        }
        public void setTime(String time)
        {
            _time = time;
        }
        public void setFolderName(String folderName)
        {
            _folderName = folderName;
        }

    }
}

