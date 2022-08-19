using Google.Protobuf;
using Grpc.Net.Client;
using gRPCFileTransportClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grpcClient
{
    public class FileTransport
    {
        private GrpcChannel channel;
        private FileService.FileServiceClient client;
        public FileTransport()
        {
            channel = GrpcChannel.ForAddress("http://localhost:5000");
            client = new FileService.FileServiceClient(channel);
        }
        public async Task FileUpload(string file)
        {
            var channel = GrpcChannel.ForAddress("http://localhost:5000");

            var client = new FileService.FileServiceClient(channel);

            FileStream fileStream = new FileStream(file, FileMode.Open);

            var content = new BytesContent
            {
                FileSize = fileStream.Length,
                ReadedByte = 0,
                Info = new gRPCFileTransportClient.FileInfo { FileName = Path.GetFileNameWithoutExtension(fileStream.Name), FileExtension = Path.GetExtension(fileStream.Name) },
            };

            var upload = client.FileUpload();

            byte[] buffer = new byte[2048];

            while ((content.ReadedByte = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                content.Buffer = ByteString.CopyFrom(buffer);
                await upload.RequestStream.WriteAsync(content);
            }

            await upload.RequestStream.CompleteAsync();

            Console.WriteLine((await upload.ResponseAsync).Message);

            fileStream.Close();
        }
    
        public async Task FileDownload(string downloadPath)
        {
            var channel = GrpcChannel.ForAddress("http://localhost:5000");

            var client = new FileService.FileServiceClient(channel);

            var fileInfo = new gRPCFileTransportClient.FileInfo
            {
                FileName = "bkg-blu",
                FileExtension = ".jpg",
            };

            FileStream fileStream = null;

            var request = client.FileDownload(fileInfo);

            var cancellation = new CancellationTokenSource();

            int count = 0;

            decimal chunkSize = 0;

            while (await request.ResponseStream.MoveNext(cancellation.Token))
            {
                if(count++ ==0)
                {
                    fileStream = new FileStream($"{downloadPath}/{request.ResponseStream.Current.Info.FileName}{request.ResponseStream.Current.Info.FileExtension}", FileMode.CreateNew);

                    fileStream.SetLength(request.ResponseStream.Current.FileSize);
                }

                var buffer = request.ResponseStream.Current.Buffer.ToByteArray();

                await fileStream.WriteAsync(buffer,0,request.ResponseStream.Current.ReadedByte);

                Console.WriteLine($"{Math.Round((chunkSize += request.ResponseStream.Current.ReadedByte * 100) / request.ResponseStream.Current.FileSize)}%");
            }
            await fileStream.DisposeAsync();
            fileStream.Close();
        }
    }
}
