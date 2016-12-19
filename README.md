# NgFlowSample

A sample ASP.NET implentation of Flow.js server side with NgFlow

To install, make sure NuGet package restore is enabled for the solution.

Launch the projec tand navigate to /index.html for the NgFlow sample Angular app. Uploads will be saved in App_Data/Tmp/FileUploads

Uploads will be written Directly to the output file with no temporary files or re-assembling of file chunks.

## Details

The Upload WebApi2 Controller is what handles the Flow.js upload. It uses the FlowUploadProcessor in /Services to conduct the upload.
The class is thread safe and has been tested with 4 simultaneous uploads with multiple threads, and implements the GET Testing
of individual chunks.

The important part of this implementation is the custom FlowMultiplartFormDataStreamProvider.cs that handles the actual streaming 
of files to disk. When it gets to the file part of the HTTP Body it retrieves the FlowMetaData from the previous content and uses
that data to save the chunk to the appropriate file.

## Advantages

1. No request memory buffering
   1. The standard HttpContext.Current.Request.Files approach first buffers the file contents into server memory before saving to disk. With 1MB chunks running on simultaneous threads, that's a lot of memory consumption

# Coming soon: Flow.Net NuGet package

Features:
1. Usable with WebApi2 or MVC5, samples included
1. No post-processing of chunks required: instead of storing each chunk on disk separately, and merging the chunks in post processing, the uploaded file is pre-allocated, and chunks are streamed into the correct place at upload time
1. Low server memory consumption: Uploads are streamed straight to disk from the network, so no server memory is used to buffer the request into memory
1. Extensible: classes interact with interfaces, and use constructor-based dependency injection, so you can provide your own implementation 
    1. Async: all interfaces return Task<T> to allow for asynchronous implementations of each class


