Imports System.Data.Entity
Imports Disco.Data.Repository
Imports Disco.BI
Imports Disco.BI.Interop.ActiveDirectory

Module ModuleMain

    <STAThread()> _
    Public Sub Main()

        'Console.WriteLine("Are you sure you want to Delete & Rebuild the Disco Database? (Y/N)")
        'Dim result = Console.ReadKey(True)
        'If result.Key = ConsoleKey.Y Then
        '    Database.SetInitializer(Of DiscoDataContext)(New DiscoDataContextInitializer)
        '    Using db As DiscoDataContext = New DiscoDataContext
        '        Console.WriteLine("Deleting Existing Database...")
        '        db.Database.Delete()
        '        Console.WriteLine("Building Database...")
        '        db.Database.Initialize(True)
        '        Console.WriteLine("Database Built")
        '    End Using
        'Else
        '    Console.WriteLine()
        '    Console.WriteLine("Cancelled")
        'End If
        'Console.WriteLine()
        'Console.WriteLine("press any key to exit")
        'Console.ReadKey(True)

        '' Move all Attachments
        'Using db As New DiscoDataContext

        '    Dim dataStoreLocation = db.DiscoConfiguration.DataStoreLocation
        '    Dim dataStoreJobAttachmentsLocation = IO.Path.Combine(dataStoreLocation, "JobAttachments")
        '    For Each ja In db.JobAttachments
        '        Dim correctFilename = ja.RepositoryFilename(db)
        '        Dim correctThumbnailFilename = ja.ThumbnailRepositoryFilename(db)

        '        Dim oldFilename = IO.Path.Combine(dataStoreJobAttachmentsLocation, String.Format("{0}_{1}_{2}", ja.JobId, ja.Id, ja.Filename))
        '        Dim oldThumbnailFilename = IO.Path.Combine(dataStoreJobAttachmentsLocation, String.Format("{0}_{1}_{2}_thumb.jpg", ja.JobId, ja.Id, ja.Filename))

        '        If Not IO.File.Exists(correctFilename) AndAlso IO.File.Exists(oldFilename) Then
        '            IO.File.Move(oldFilename, correctFilename)
        '        End If
        '        If Not IO.File.Exists(correctThumbnailFilename) AndAlso IO.File.Exists(oldThumbnailFilename) Then
        '            IO.File.Move(oldThumbnailFilename, correctThumbnailFilename)
        '        End If
        '    Next


        '    Dim dataStoreUserAttachmentsLocation = IO.Path.Combine(dataStoreLocation, "UserAttachments")
        '    For Each u In db.UserAttachments
        '        Dim correctFilename = u.RepositoryFilename(db)
        '        Dim correctThumbnailFilename = u.ThumbnailRepositoryFilename(db)

        '        Dim oldFilename = IO.Path.Combine(dataStoreUserAttachmentsLocation, String.Format("{0}_{1}_{2}", u.UserId, u.Id, u.Filename))
        '        Dim oldThumbnailFilename = IO.Path.Combine(dataStoreUserAttachmentsLocation, String.Format("{0}_{1}_{2}_thumb.jpg", u.UserId, u.Id, u.Filename))

        '        If Not IO.File.Exists(correctFilename) AndAlso IO.File.Exists(oldFilename) Then
        '            Console.WriteLine("Moving {0}_{1}_{2}", u.UserId, u.Id, u.Filename)
        '            IO.File.Move(oldFilename, correctFilename)
        '        End If
        '        If Not IO.File.Exists(correctThumbnailFilename) AndAlso IO.File.Exists(oldThumbnailFilename) Then
        '            Console.WriteLine("Moving {0}_{1}_{2}_thumb.jpg", u.UserId, u.Id, u.Filename)
        '            IO.File.Move(oldThumbnailFilename, correctThumbnailFilename)
        '        End If
        '    Next


        '    Dim dataStoreDeviceAttachmentsLocation = IO.Path.Combine(dataStoreLocation, "DeviceAttachments")
        '    For Each da In db.DeviceAttachments
        '        Dim correctFilename = da.RepositoryFilename(db)
        '        Dim correctThumbnailFilename = da.ThumbnailRepositoryFilename(db)

        '        Dim oldFilename = IO.Path.Combine(dataStoreDeviceAttachmentsLocation, String.Format("{0}_{1}_{2}", da.DeviceSerialNumber, da.Id, da.Filename))
        '        Dim oldThumbnailFilename = IO.Path.Combine(dataStoreDeviceAttachmentsLocation, String.Format("{0}_{1}_{2}_thumb.jpg", da.DeviceSerialNumber, da.Id, da.Filename))

        '        If Not IO.File.Exists(correctFilename) AndAlso IO.File.Exists(oldFilename) Then
        '            Console.WriteLine("Moving {0}_{1}_{2}", da.DeviceSerialNumber, da.Id, da.Filename)
        '            IO.File.Move(oldFilename, correctFilename)
        '        End If
        '        If Not IO.File.Exists(correctThumbnailFilename) AndAlso IO.File.Exists(oldThumbnailFilename) Then
        '            Console.WriteLine("Moving {0}_{1}_{2}_thumb.jpg", da.DeviceSerialNumber, da.Id, da.Filename)
        '            IO.File.Move(oldThumbnailFilename, correctThumbnailFilename)
        '        End If
        '    Next

        '    Dim devices = db.Devices.Include("AssignedUser").Include("DeviceModel").Include("DeviceProfile")
        '    For Each d In devices
        '        If Not String.IsNullOrEmpty(d.ComputerName) AndAlso d.ComputerName.Length <= 24 Then

        '            Dim adMachineAccount = Interop.ActiveDirectory.ActiveDirectory.GetMachineAccount(d.ComputerName)
        '            If adMachineAccount IsNot Nothing Then
        '                Console.WriteLine("Updating: {0}", d.SerialNumber)
        '                adMachineAccount.SetDescription(d)
        '            End If

        '        End If
        '    Next

        '    FixAttachmentFilenames(IO.Path.Combine(dataStoreLocation, "UserAttachments"))
        '    FixAttachmentFilenames(IO.Path.Combine(dataStoreLocation, "JobAttachments"))
        '    FixAttachmentFilenames(IO.Path.Combine(dataStoreLocation, "DeviceAttachments"))

        '    Console.WriteLine("Finished")

        'End Using

    End Sub

    Sub FixAttachmentFilenames(Path As String)
        For Each f In IO.Directory.EnumerateFiles(Path)
            Dim fS = f.Split("_")
            If fS.Length >= 2 AndAlso Not f.EndsWith("file") Then
                Dim newFilename As String
                If f.EndsWith("_thumb.jpg") Then
                    newFilename = String.Format("{0}_{1}_thumb.jpg", fS(0), fS(1))
                Else
                    newFilename = String.Format("{0}_{1}_file", fS(0), fS(1))
                End If
                Console.WriteLine(newFilename)
                IO.File.Move(f, IO.Path.Combine(IO.Path.GetDirectoryName(f), newFilename))
            End If
        Next

        For Each d In IO.Directory.EnumerateDirectories(Path)
            FixAttachmentFilenames(d)
        Next
    End Sub

End Module
