// WiredTigerNet.h

#pragma once

using namespace System;
using namespace System::Runtime::InteropServices;

namespace WiredTigerNet {

	public ref class WiredException : Exception
	{
		int _tigerError;
		String^ _message;
	public:
		WiredException(int tigerError) : _tigerError(tigerError)
		{
			const char *err = wiredtiger_strerror(tigerError);
			_message = String::Format("WiredTiger Error {0} : [{1}]", tigerError, gcnew String(err));
		}

		property int TigerError {
			int get() { return _tigerError; }
		}

		property String^ Message { 
			String^ get() override
			{
				return _message;
			}
		}
	};

	ref class Session;
	public ref class Cursor : public IDisposable
	{
		Session^ _session;
		WT_CURSOR* _cursor;
	internal:
		Cursor(Session^ session, WT_CURSOR* cursor) : _session(session), _cursor(cursor){}
	public:
		~Cursor()
		{
			_session = nullptr;
			if (_cursor != NULL)
			{
				_cursor->close(_cursor);
				_cursor = NULL;
			}
		}
		inline Cursor^ Duplicate();

		array<Byte>^ GetKey()
		{
			WT_ITEM item = { 0 };
			int r=_cursor->get_key(_cursor, &item);
			if (r != 0) throw gcnew WiredException(r);

			array<Byte>^ buffer = gcnew array<Byte>(item.size);
			Marshal::Copy((IntPtr)(void*)item.data, buffer, 0, buffer->Length);
			return buffer;
		}
		array<Byte>^ GetValue()
		{
			WT_ITEM item = { 0 };
			int r =_cursor->get_value(_cursor, &item);
			if (r != 0) throw gcnew WiredException(r);

			array<Byte>^ buffer = gcnew array<Byte>(item.size);
			Marshal::Copy((IntPtr)(void*)item.data, buffer, 0, buffer->Length);
			return buffer;
		}

		void SetKey(IntPtr data, int length)
		{
			WT_ITEM item = { 0 };
			item.data = (void*)data;
			item.size = length;
			_cursor->set_key(_cursor, &item);
		}
		void SetKey(array<Byte>^ key)
		{
			pin_ptr<Byte> pkey = &key[0];
			WT_ITEM item = { 0 };
			item.data = (void*)pkey;
			item.size = key->Length;
			_cursor->set_key(_cursor, &item);
		}

		void SetValue(IntPtr data, int length)
		{
			WT_ITEM item = { 0 };
			item.data = (void*)data;
			item.size = length;
			_cursor->set_value(_cursor, &item);
		}
		void SetValue(array<Byte>^ value)
		{
			pin_ptr<Byte> pvalue = &value[0];
			WT_ITEM item = { 0 };
			item.data = (void*)pvalue;
			item.size = value->Length;
			_cursor->set_value(_cursor, &item);
		}

		void Insert()
		{
			int r = _cursor->insert(_cursor);
			if (r != 0) throw gcnew WiredException(r);
		}

		bool Next()
		{
			int r = _cursor->next(_cursor);
			if (r == WT_NOTFOUND) return false;
			if (r != 0) throw gcnew WiredException(r);
			return true;
		}

		bool Prev()
		{
			int r = _cursor->prev(_cursor);
			if (r == WT_NOTFOUND) return false;
			if (r != 0) throw gcnew WiredException(r);
			return true;
		}

		void Remove()
		{
			int r = _cursor->remove(_cursor);
			if (r != 0) throw gcnew WiredException(r);
		}
		void Reset()
		{
			int r = _cursor->reset(_cursor);
			if (r != 0) throw gcnew WiredException(r);
		}

		bool Search()
		{
			int r = _cursor->search(_cursor);
			if (r == WT_NOTFOUND) return false;
			if (r != 0) throw gcnew WiredException(r);
			return true;
		}

		int SearchNear()
		{
			int exact;
			int r = _cursor->search_near(_cursor,&exact);
			if (r != 0) throw gcnew WiredException(r);
			return exact;
		}

		void Update()
		{
			int r = _cursor->update(_cursor);
			if (r != 0) throw gcnew WiredException(r);
		}
	};



	public ref class Session : public IDisposable
	{
		WT_SESSION *_session;
	internal:
		Session(WT_SESSION *session) : _session(session)
		{}


		Cursor^ DuplicateCursor(WT_CURSOR* cursor)
		{
			WT_CURSOR *newcursor;
			int r = _session->open_cursor(_session, NULL, cursor, NULL, &cursor);
			if (r != 0) throw gcnew WiredException(r);
			return gcnew Cursor(this, cursor);
		}

	public:	
		~Session()
		{
			if (_session != NULL)
			{
				_session->close(_session, NULL);
				_session = NULL;
			}
		}

		void BeginTran()
		{
			int r = _session->begin_transaction(_session, NULL);
			if (r != 0) throw gcnew WiredException(r);
		}

		void BeginTran(String^ config)
		{
			const char *aconfig = (char*)Marshal::StringToHGlobalAnsi(config).ToPointer();
			int r = _session->begin_transaction(_session, aconfig);
			Marshal::FreeHGlobal((IntPtr)(void*)aconfig);
			if (r != 0) throw gcnew WiredException(r);
		}

		void CommitTran()
		{
			int r = _session->commit_transaction(_session, NULL);
			if (r != 0) throw gcnew WiredException(r);
		}
		void RollbackTran()
		{
			int r = _session->rollback_transaction(_session, NULL);
			if (r != 0) throw gcnew WiredException(r);
		}

		void Checkpoint()
		{
			int r = _session->checkpoint(_session, NULL);
			if (r != 0) throw gcnew WiredException(r);
		}
		void Checkpoint(String^ config)
		{
			const char *aconfig = (char*)Marshal::StringToHGlobalAnsi(config).ToPointer();
			int r = _session->checkpoint(_session, aconfig);
			Marshal::FreeHGlobal((IntPtr)(void*)aconfig);
			if (r != 0) throw gcnew WiredException(r);
		}

		void Compact(String^ name)
		{
			const char *aname = (char*)Marshal::StringToHGlobalAnsi(name).ToPointer();
			int r = _session->compact(_session, aname,NULL);
			Marshal::FreeHGlobal((IntPtr)(void*)aname);
			if (r != 0) throw gcnew WiredException(r);
		}

		void Create(String^ name, String^ config)
		{
			const char *aname = (char*)Marshal::StringToHGlobalAnsi(name).ToPointer();
			const char *aconfig = (char*)Marshal::StringToHGlobalAnsi(config).ToPointer();
			int r = _session->create(_session, aname, aconfig);
			Marshal::FreeHGlobal((IntPtr)(void*)aname);
			Marshal::FreeHGlobal((IntPtr)(void*)aconfig);
			if (r != 0) throw gcnew WiredException(r);
		}

		void Drop(String^ name)
		{
			const char *aname = (char*)Marshal::StringToHGlobalAnsi(name).ToPointer();
			int r = _session->drop(_session, aname, NULL);
			Marshal::FreeHGlobal((IntPtr)(void*)aname);
			if (r != 0) throw gcnew WiredException(r);
		}

		void Rename(String^ oldname, String^ newname)
		{
			const char *aoldname = (char*)Marshal::StringToHGlobalAnsi(oldname).ToPointer();
			const char *anewname = (char*)Marshal::StringToHGlobalAnsi(newname).ToPointer();
			int r = _session->rename(_session, aoldname, anewname, NULL);
			Marshal::FreeHGlobal((IntPtr)(void*)aoldname);
			Marshal::FreeHGlobal((IntPtr)(void*)anewname);
			if (r != 0) throw gcnew WiredException(r);
		}

		Cursor^ OpenCursor(String^ name)
		{
			WT_CURSOR *cursor;
			const char *aname = (char*)Marshal::StringToHGlobalAnsi(name).ToPointer();
			int r = _session->open_cursor(_session, aname, NULL, NULL, &cursor);
			Marshal::FreeHGlobal((IntPtr)(void*)aname);
			if (r != 0) throw gcnew WiredException(r);

			return gcnew Cursor(this, cursor);
		}

	};

	Cursor^ Cursor::Duplicate()
	{
		return _session->DuplicateCursor(_cursor);
	}


	public ref class Connection : public IDisposable
	{
		WT_CONNECTION *_connection;
		
		Connection(){}

	public:
		~Connection()
		{
			if (_connection != NULL)
			{
				_connection->close(_connection, NULL);
				_connection = NULL;
			}
		}

		Session^ OpenSession(String^ config)
		{
			const char *aconfig = (char*)Marshal::StringToHGlobalAnsi(config).ToPointer();
			WT_SESSION *session;
			int r = _connection->open_session(_connection, NULL, aconfig, &session);
			Marshal::FreeHGlobal((IntPtr)(void*)aconfig);
			if (r != 0) throw gcnew WiredException(r);

			return gcnew Session(session);
		}

		bool IsNew()
		{
			return _connection->is_new(_connection)?true:false;
		}

		String^ GetHome()
		{
			const char *home = _connection->get_home(_connection);
			return gcnew String(home);
		}

		void AsyncFlush()
		{
			int r = _connection->async_flush(_connection);
			if (r != 0) throw gcnew WiredException(r);
		}


		static Connection^ Open(String^ home, String^ config)
		{
			WT_CONNECTION *connectionp;

			const char *ahome = (char*)Marshal::StringToHGlobalAnsi(home).ToPointer();
			const char *aconfig = (char*)Marshal::StringToHGlobalAnsi(config).ToPointer();

			Connection^ ret = gcnew Connection();

			int r= wiredtiger_open(ahome,NULL /*WT_EVENT_HANDLER *errhandler*/, aconfig,&connectionp);
			Marshal::FreeHGlobal((IntPtr)(void*)ahome);
			Marshal::FreeHGlobal((IntPtr)(void*)aconfig);
			if (r != 0) throw gcnew WiredException(r);

			ret->_connection = connectionp;

			return ret;
		}
	};

}
