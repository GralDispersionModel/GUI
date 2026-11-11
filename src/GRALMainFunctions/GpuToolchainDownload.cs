#region Copyright
///<remarks>
/// <GRAL Graphical User Interface GUI>
/// Copyright (C) [2019]  [Dietmar Oettl, Markus Kuntner]
/// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
/// the Free Software Foundation version 3 of the License
/// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
/// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.
///</remarks>
#endregion

using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;

namespace Gral
{
    /// <summary>
    /// A runtime compiler for the GPU build of GRAL (NVRTC or hiprtc), taken from the vendor's Python package.
    /// Only the needed DLLs are read from the package with HTTP range requests (the whole package if the server does not
    /// support them), each one is checked against its SHA-256, and the set is installed into
    /// %LOCALAPPDATA%\GRAL\GpuToolchain\(nvrtc|hiprtc), where GRAL looks for it without administrator rights.
    /// </summary>
    internal sealed class GpuToolchainPackage
    {
        public string Kind;
        public string Title;
        public string Url;
        public string LicenseUrl;
        public long DownloadBytes;
        public (string Entry, string Name, string Sha256)[] Files;

        /// <summary>
        /// NVRTC 12.9 from nvidia-cuda-nvrtc-cu12 12.9.86 on PyPI (the version GRAL was tested with)
        /// </summary>
        public static readonly GpuToolchainPackage Nvrtc = new GpuToolchainPackage
        {
            Kind = "nvrtc",
            Title = "NVIDIA NVRTC 12.9 (CUDA runtime compiler, package nvidia-cuda-nvrtc-cu12 12.9.86 from PyPI)",
            Url = "https://files.pythonhosted.org/packages/52/de/823919be3b9d0ccbf1f784035423c5f18f4267fb0123558d58b813c6ec86/nvidia_cuda_nvrtc_cu12-12.9.86-py3-none-win_amd64.whl",
            LicenseUrl = "https://docs.nvidia.com/cuda/eula/index.html",
            DownloadBytes = 37328080 + 1695815,
            Files = new[]
            {
                ("nvidia/cuda_nvrtc/bin/nvrtc64_120_0.dll", "nvrtc64_120_0.dll", "0cc63349fedffeec1660b30955e8028b6e4e4c26cf1e41c56dc4569fd5eca82c"),
                ("nvidia/cuda_nvrtc/bin/nvrtc-builtins64_129.dll", "nvrtc-builtins64_129.dll", "c405226b0b394ed8bdc593bb1419b94a989f93248ed2873e8f2e0e27c1688ca1"),
            }
        };

        /// <summary>
        /// hiprtc 0716 with amd_comgr from rocm-sdk-core 10.1.0 of AMD ROCm (the version GRAL was tested with)
        /// </summary>
        public static readonly GpuToolchainPackage Hiprtc = new GpuToolchainPackage
        {
            Kind = "hiprtc",
            Title = "AMD hiprtc with amd_comgr (HIP runtime compiler, package rocm-sdk-core 10.1.0 from AMD ROCm)",
            Url = "https://stable.repo.amd.com/rocm/core/whl-next/rocm-sdk-core/rocm_sdk_core-10.1.0-py3-none-win_amd64.whl",
            LicenseUrl = "https://rocm.docs.amd.com/en/latest/about/license.html",
            DownloadBytes = 43896638 + 325570 + 160765,
            Files = new[]
            {
                ("_rocm_sdk_core/bin/hiprtc0716.dll", "hiprtc0716.dll", "ee7d306880167ae3c0cec7e27953d14ec14d75576d3ba6fab43497dfa873cde3"),
                ("_rocm_sdk_core/bin/hiprtc-builtins0716.dll", "hiprtc-builtins0716.dll", "ad40c1bec5fbeb8b9f1f765207d0301982e674766bf86fb0a82fe4b3d7ea9653"),
                ("_rocm_sdk_core/bin/amd_comgr.dll", "amd_comgr.dll", "da9527847c99efd3df99bd46fe24abae7920b04df0f51e7c677abcccd4924e58"),
            }
        };

        /// <summary>
        /// Folder that GRAL searches last for this compiler
        /// </summary>
        public string TargetFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GRAL", "GpuToolchain", Kind);

        /// <summary>
        /// Download, check and install the compiler
        /// </summary>
        /// <param name="Progress">Called with the number of bytes received so far</param>
        /// <param name="Token">Cancellation</param>
        public void Install(Action<long> Progress, CancellationToken Token)
        {
            string target = TargetFolder;
            string part = target + ".part";
            if (Directory.Exists(part))
            {
                Directory.Delete(part, true);
            }
            Directory.CreateDirectory(part);
            try
            {
                using (HttpClient client = CreateClient())
                using (Stream package = OpenPackage(client, Progress, Token))
                using (ZipArchive zip = new ZipArchive(package, ZipArchiveMode.Read))
                {
                    foreach (var file in Files)
                    {
                        ZipArchiveEntry entry = zip.GetEntry(file.Entry) ?? throw new InvalidDataException(file.Entry + " is missing in " + Url);
                        string path = Path.Combine(part, file.Name);
                        using (Stream input = entry.Open())
                        using (FileStream output = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
                        using (IncrementalHash sha = IncrementalHash.CreateHash(HashAlgorithmName.SHA256))
                        {
                            byte[] buffer = new byte[1 << 16];
                            int read;
                            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                Token.ThrowIfCancellationRequested();
                                sha.AppendData(buffer, 0, read);
                                output.Write(buffer, 0, read);
                            }
                            string hash = Convert.ToHexString(sha.GetHashAndReset());
                            if (!hash.Equals(file.Sha256, StringComparison.OrdinalIgnoreCase))
                            {
                                throw new InvalidDataException(file.Name + " has an unexpected SHA-256 (" + hash + ")");
                            }
                        }
                    }
                }
                if (Directory.Exists(target))
                {
                    Directory.Delete(target, true);
                }
                Directory.Move(part, target);
            }
            finally
            {
                try
                {
                    if (Directory.Exists(part))
                    {
                        Directory.Delete(part, true);
                    }
                }
                catch { }
            }
        }

        private static HttpClient CreateClient()
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                UseProxy = true,
                Proxy = null,
                UseDefaultCredentials = true,
                AllowAutoRedirect = true
            };
#if !__MonoCS__
            handler.DefaultProxyCredentials = System.Net.CredentialCache.DefaultNetworkCredentials;
#endif
            HttpClient client = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(10) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("GRAL-GUI");
            return client;
        }

        /// <summary>
        /// A seekable stream over the package: ranges on demand, or the whole package in a temporary file
        /// </summary>
        private Stream OpenPackage(HttpClient Client, Action<long> Progress, CancellationToken Token)
        {
            using (HttpRequestMessage probe = new HttpRequestMessage(HttpMethod.Get, Url))
            {
                probe.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(0, 0);
                using (HttpResponseMessage response = Client.Send(probe, HttpCompletionOption.ResponseHeadersRead, Token))
                {
                    response.EnsureSuccessStatusCode();
                    long? length = response.Content.Headers.ContentRange?.Length;
                    if (response.StatusCode == System.Net.HttpStatusCode.PartialContent && length > 0)
                    {
                        return new HttpRangeStream(Client, response.RequestMessage.RequestUri, length.Value, Progress, Token);
                    }
                }
            }
            // No range support: download the whole package
            FileStream file = new FileStream(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.None, 1 << 16, FileOptions.DeleteOnClose);
            try
            {
                using (HttpResponseMessage response = Client.Send(new HttpRequestMessage(HttpMethod.Get, Url), HttpCompletionOption.ResponseHeadersRead, Token))
                {
                    response.EnsureSuccessStatusCode();
                    using (Stream input = response.Content.ReadAsStream(Token))
                    {
                        byte[] buffer = new byte[1 << 16];
                        long total = 0;
                        int read;
                        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            Token.ThrowIfCancellationRequested();
                            file.Write(buffer, 0, read);
                            total += read;
                            Progress(total);
                        }
                    }
                }
                file.Position = 0;
                return file;
            }
            catch
            {
                file.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Read-only stream that fetches 1 MiB blocks of a remote file with HTTP range requests
        /// </summary>
        private sealed class HttpRangeStream : Stream
        {
            private const int BlockSize = 1 << 20;
            private readonly HttpClient client;
            private readonly Uri uri;
            private readonly long length;
            private readonly Action<long> progress;
            private readonly CancellationToken token;
            private readonly byte[] block = new byte[BlockSize];
            private long blockStart = -1;
            private int blockLength;
            private long position;
            private long received;

            public HttpRangeStream(HttpClient Client, Uri Uri, long Length, Action<long> Progress, CancellationToken Token)
            {
                client = Client;
                uri = Uri;
                length = Length;
                progress = Progress;
                token = Token;
            }

            public override bool CanRead => true;
            public override bool CanSeek => true;
            public override bool CanWrite => false;
            public override long Length => length;
            public override long Position { get => position; set => position = value; }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (position >= length || count == 0)
                {
                    return 0;
                }
                if (blockStart < 0 || position < blockStart || position >= blockStart + blockLength)
                {
                    Fetch(position);
                }
                int n = (int)Math.Min(count, blockStart + blockLength - position);
                Buffer.BlockCopy(block, (int)(position - blockStart), buffer, offset, n);
                position += n;
                return n;
            }

            private void Fetch(long start)
            {
                token.ThrowIfCancellationRequested();
                long end = Math.Min(start + BlockSize, length) - 1;
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(start, end);
                    using (HttpResponseMessage response = client.Send(request, HttpCompletionOption.ResponseHeadersRead, token))
                    {
                        if (response.StatusCode != System.Net.HttpStatusCode.PartialContent || response.Content.Headers.ContentRange?.From != start)
                        {
                            throw new IOException("The server did not return the requested part of " + uri);
                        }
                        using (Stream input = response.Content.ReadAsStream(token))
                        {
                            int filled = 0, read;
                            int wanted = (int)(end - start + 1);
                            while (filled < wanted && (read = input.Read(block, filled, wanted - filled)) > 0)
                            {
                                filled += read;
                            }
                            if (filled != wanted)
                            {
                                throw new IOException("Incomplete part of " + uri);
                            }
                            blockStart = start;
                            blockLength = filled;
                            received += filled;
                            progress(received);
                        }
                    }
                }
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                position = origin == SeekOrigin.Begin ? offset : origin == SeekOrigin.Current ? position + offset : length + offset;
                return position;
            }

            public override void Flush() { }
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        }
    }
}
