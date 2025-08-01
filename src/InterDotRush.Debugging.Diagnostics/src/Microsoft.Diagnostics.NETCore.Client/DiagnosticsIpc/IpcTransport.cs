﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.NETCore.Client
{
    internal abstract class IpcEndpoint
    {
        /// <summary>
        /// Connects to the underlying IPC transport and opens a read/write-able Stream
        /// </summary>
        /// <param name="timeout">The amount of time to block attempting to connect</param>
        /// <returns>A stream used for writing and reading data to and from the target .NET process</returns>
        /// <throws>ServerNotAvailableException</throws>
        public abstract Stream Connect(TimeSpan timeout);

        /// <summary>
        /// Connects to the underlying IPC transport and opens a read/write-able Stream
        /// </summary>
        /// <param name="token">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that completes with a stream used for writing and reading data to and from the target .NET process.
        /// </returns>
        /// <throws>ServerNotAvailableException</throws>
        public abstract Task<Stream> ConnectAsync(CancellationToken token);

        /// <summary>
        /// Wait for an available diagnostic endpoint to the runtime instance.
        /// </summary>
        /// <param name="timeout">The amount of time to wait before cancelling the wait for the connection.</param>
        public abstract void WaitForConnection(TimeSpan timeout);

        /// <summary>
        /// Wait for an available diagnostic endpoint to the runtime instance.
        /// </summary>
        /// <param name="token">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that completes when a diagnostic endpoint to the runtime instance becomes available.
        /// </returns>
        public abstract Task WaitForConnectionAsync(CancellationToken token);
    }

    internal static class IpcEndpointHelper
    {
        public static Stream Connect(IpcEndpointConfig config, TimeSpan timeout)
        {
            try
            {
                if (config.Transport == IpcEndpointConfig.TransportType.NamedPipe)
                {
                    NamedPipeClientStream namedPipe = new(
                        ".",
                        config.Address,
                        PipeDirection.InOut,
                        PipeOptions.None,
                        TokenImpersonationLevel.Impersonation);
                    namedPipe.Connect((int)timeout.TotalMilliseconds);
                    return namedPipe;
                }
                else if (config.Transport == IpcEndpointConfig.TransportType.UnixDomainSocket)
                {
                    IpcUnixDomainSocket socket = new();
                    socket.Connect(new IpcUnixDomainSocketEndPoint(config.Address), timeout);
                    return new ExposedSocketNetworkStream(socket, ownsSocket: true);
                }
#if DIAGNOSTICS_RUNTIME
                else if (config.Transport == IpcEndpointConfig.TransportType.TcpSocket)
                {
                    var tcpClient = new TcpClient ();
                    var endPoint = new IpcTcpSocketEndPoint(config.Address);
                    tcpClient.Connect(endPoint.EndPoint);
                    return tcpClient.GetStream();
                }
#endif
                else
                {
                    throw new ArgumentException($"Unsupported IpcEndpointConfig transport type {config.Transport}");
                }

            }
            catch (SocketException ex)
            {
                throw new ServerNotAvailableException($"Unable to connect to the server. {ex.Message}", ex);
            }
        }

        public static async Task<Stream> ConnectAsync(IpcEndpointConfig config, CancellationToken token)
        {
            try
            {
                if (config.Transport == IpcEndpointConfig.TransportType.NamedPipe)
                {
                    NamedPipeClientStream namedPipe = new(
                        ".",
                        config.Address,
                        PipeDirection.InOut,
                        PipeOptions.Asynchronous,
                        TokenImpersonationLevel.Impersonation);

                    // Pass non-infinite timeout in order to cause internal connection algorithm
                    // to check the CancellationToken periodically. Otherwise, if the named pipe
                    // is waited using WaitNamedPipe with an infinite timeout, then the
                    // CancellationToken cannot be observed.
                    await namedPipe.ConnectAsync(int.MaxValue, token).ConfigureAwait(false);

                    return namedPipe;
                }
                else if (config.Transport == IpcEndpointConfig.TransportType.UnixDomainSocket)
                {
                    IpcUnixDomainSocket socket = new();
                    await socket.ConnectAsync(new IpcUnixDomainSocketEndPoint(config.Address), token).ConfigureAwait(false);
                    return new ExposedSocketNetworkStream(socket, ownsSocket: true);
                }
                else
                {
                    throw new ArgumentException($"Unsupported IpcEndpointConfig transport type {config.Transport}");
                }
            }
            catch (SocketException ex)
            {
                throw new ServerNotAvailableException($"Unable to connect to the server. {ex.Message}", ex);
            }
        }
    }

    internal class ServerIpcEndpoint : IpcEndpoint
    {
        private readonly Guid _runtimeId;
        private readonly ReversedDiagnosticsServer _server;

        public ServerIpcEndpoint(ReversedDiagnosticsServer server, Guid runtimeId)
        {
            _runtimeId = runtimeId;
            _server = server;
        }

        /// <remarks>
        /// This will block until the diagnostic stream is provided. This block can happen if
        /// the stream is acquired previously and the runtime instance has not yet reconnected
        /// to the reversed diagnostics server.
        /// </remarks>
        public override Stream Connect(TimeSpan timeout)
        {
            return _server.Connect(_runtimeId, timeout);
        }

        public override Task<Stream> ConnectAsync(CancellationToken token)
        {
            return _server.ConnectAsync(_runtimeId, token);
        }

        public override void WaitForConnection(TimeSpan timeout)
        {
            _server.WaitForConnection(_runtimeId, timeout);
        }

        public override Task WaitForConnectionAsync(CancellationToken token)
        {
            return _server.WaitForConnectionAsync(_runtimeId, token);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ServerIpcEndpoint);
        }

        public bool Equals(ServerIpcEndpoint other)
        {
            return other != null && other._runtimeId == _runtimeId && other._server == _server;
        }

        public override int GetHashCode()
        {
            return _runtimeId.GetHashCode() ^ _server.GetHashCode();
        }
    }

    internal class DiagnosticPortIpcEndpoint : IpcEndpoint
    {
        private IpcEndpointConfig _config;

        public DiagnosticPortIpcEndpoint(string diagnosticPort)
        {
            _config = IpcEndpointConfig.Parse(diagnosticPort);
        }

        public DiagnosticPortIpcEndpoint(IpcEndpointConfig config)
        {
            _config = config;
        }

        public override Stream Connect(TimeSpan timeout)
        {
            return IpcEndpointHelper.Connect(_config, timeout);
        }

        public override async Task<Stream> ConnectAsync(CancellationToken token)
        {
            return await IpcEndpointHelper.ConnectAsync(_config, token).ConfigureAwait(false);
        }

        public override void WaitForConnection(TimeSpan timeout)
        {
            using Stream _ = Connect(timeout);
        }

        public override async Task WaitForConnectionAsync(CancellationToken token)
        {
            using Stream _ = await ConnectAsync(token).ConfigureAwait(false);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DiagnosticPortIpcEndpoint);
        }

        public bool Equals(DiagnosticPortIpcEndpoint other)
        {
            return other != null && other._config == _config;
        }

        public override int GetHashCode()
        {
            return _config.GetHashCode();
        }
    }

    internal class PidIpcEndpoint : IpcEndpoint
    {
        public static string IpcRootPath { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"\\.\pipe\" : Path.GetTempPath();
        public static string DiagnosticsPortPattern { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"^dotnet-diagnostic-(\d+)$" : @"^dotnet-diagnostic-(\d+)-(\d+)-socket$";
        // Format strings as private const members
        private const string _defaultAddressFormatWindows = "dotnet-diagnostic-{0}";
        private const string _dsrouterAddressFormatWindows = "dotnet-diagnostic-dsrouter-{0}";
        private const string _defaultAddressFormatNonWindows = "dotnet-diagnostic-{0}-{1}-socket";
        private const string _dsrouterAddressFormatNonWindows = "dotnet-diagnostic-dsrouter-{0}-{1}-socket";
        private int _pid;
        private IpcEndpointConfig _config;
        /// <summary>
        /// Creates a reference to a .NET process's IPC Transport
        /// using the default rules for a given pid
        /// </summary>
        /// <param name="pid">The pid of the target process</param>
        /// <returns>A reference to the IPC Transport</returns>
        public PidIpcEndpoint(int pid)
        {
            _pid = pid;
        }

        public override Stream Connect(TimeSpan timeout)
        {
            string address = GetDefaultAddress();
            _config = IpcEndpointConfig.Parse(address + ",connect");
            return IpcEndpointHelper.Connect(_config, timeout);
        }

        public override async Task<Stream> ConnectAsync(CancellationToken token)
        {
            string address = GetDefaultAddress();
            _config = IpcEndpointConfig.Parse(address + ",connect");
            return await IpcEndpointHelper.ConnectAsync(_config, token).ConfigureAwait(false);
        }

        public override void WaitForConnection(TimeSpan timeout)
        {
            using Stream _ = Connect(timeout);
        }

        public override async Task WaitForConnectionAsync(CancellationToken token)
        {
            using Stream _ = await ConnectAsync(token).ConfigureAwait(false);
        }

        private string GetDefaultAddress()
        {
            return GetDefaultAddress(_pid);
        }

        private static bool TryGetDefaultAddress(int pid, out string defaultAddress)
        {
            defaultAddress = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                defaultAddress = string.Format(_defaultAddressFormatWindows, pid);

                try
                {
                    string dsrouterAddress = Directory.GetFiles(IpcRootPath, string.Format(_dsrouterAddressFormatWindows, pid)).FirstOrDefault();
                    if (!string.IsNullOrEmpty(dsrouterAddress))
                    {
                        defaultAddress = dsrouterAddress;
                    }
                }
                catch { }
            }
            else
            {
                try
                {
                    defaultAddress = Directory.GetFiles(IpcRootPath, string.Format(_defaultAddressFormatNonWindows, pid, "*")) // Try best match.
                        .OrderByDescending(f => new FileInfo(f).LastWriteTime)
                        .FirstOrDefault();

                    string dsrouterAddress = Directory.GetFiles(IpcRootPath, string.Format(_dsrouterAddressFormatNonWindows, pid, "*")) // Try best match.
                        .OrderByDescending(f => new FileInfo(f).LastWriteTime)
                        .FirstOrDefault();

                    if (!string.IsNullOrEmpty(dsrouterAddress) && !string.IsNullOrEmpty(defaultAddress))
                    {
                        FileInfo defaultFile = new(defaultAddress);
                        FileInfo dsrouterFile = new(dsrouterAddress);

                        if (dsrouterFile.LastWriteTime >= defaultFile.LastWriteTime)
                        {
                            defaultAddress = dsrouterAddress;
                        }
                    }
                }
                catch { }
            }

            return !string.IsNullOrEmpty(defaultAddress);
        }

        public static string GetDefaultAddress(int pid)
        {
            try
            {
                Process process = Process.GetProcessById(pid);
            }
            catch (ArgumentException)
            {
                throw new ServerNotAvailableException($"Process {pid} is not running.");
            }
            catch (InvalidOperationException)
            {
                throw new ServerNotAvailableException($"Process {pid} seems to be elevated.");
            }

            if (!TryGetDefaultAddress(pid, out string defaultAddress))
            {
                string msg = $"Unable to connect to Process {pid}.";
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    int total_length = IpcRootPath.Length + string.Format(_defaultAddressFormatNonWindows, pid, "##########").Length;
                    if (total_length > 108) // This isn't perfect as we don't know the disambiguation key length. However it should catch most cases.
                    {
                        msg += "The total length of the diagnostic socket path may exceed 108 characters. " +
                            "Try setting the TMPDIR environment variable to a shorter path";
                    }
                    msg += $" Please verify that {IpcRootPath} is writable by the current user. "
                        + "If the target process has environment variable TMPDIR set, please set TMPDIR to the same directory. "
                        + "Please also ensure that the target process has {TMPDIR}/dotnet-diagnostic-{pid}-{disambiguation_key}-socket shorter than 108 characters. "
                        + "Please see https://aka.ms/dotnet-diagnostics-port for more information";
                }
                throw new ServerNotAvailableException(msg);
            }

            return defaultAddress;
        }

        public static bool IsDefaultAddressDSRouter(int pid, string address)
        {
            if (address.StartsWith(IpcRootPath, StringComparison.OrdinalIgnoreCase))
            {
                address = address.Substring(IpcRootPath.Length);
            }

            string dsrouterAddress = string.Format(_dsrouterAddressFormatWindows, pid);
            return address.StartsWith(dsrouterAddress, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PidIpcEndpoint);
        }

        public bool Equals(PidIpcEndpoint other)
        {
            return other != null && other._pid == _pid;
        }

        public override int GetHashCode()
        {
            return _pid.GetHashCode();
        }
    }
}
