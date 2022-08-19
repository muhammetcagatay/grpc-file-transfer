using grpcClient;

FileTransport fileTransport = new FileTransport();

//await fileTransport.FileUpload(@"C:/Users/muham/Documents/My Web Sites/WebSite1/bkg-blu.jpg");

await fileTransport.FileDownload(@"D:/Projects/.Net/gRPC/grpcClient/DownloadFiles");