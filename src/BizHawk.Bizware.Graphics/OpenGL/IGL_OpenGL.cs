using System;
using System.Drawing;
using System.Numerics;

using Silk.NET.OpenGL;

namespace BizHawk.Bizware.Graphics
{
	/// <summary>
	/// OpenGL implementation of the IGL interface
	/// </summary>
	public class IGL_OpenGL : IGL
	{
		public EDispMethod DispMethodEnum => EDispMethod.OpenGL;

		private readonly GL GL;

		// rendering state
		private OpenGLPipeline _curPipeline;
		internal bool DefaultRenderTargetBound;

		// this IGL either requires at least OpenGL 3.0
		public static bool Available => OpenGLVersion.SupportsVersion(3, 0);

		public IGL_OpenGL()
		{
			if (!Available)
			{
				throw new InvalidOperationException("OpenGL 3.0 is required and unavailable");
			}

			GL = GL.GetApi(SDL2OpenGLContext.GetGLProcAddress);
		}

		public void Dispose()
			=> GL.Dispose();

		public void ClearColor(Color color)
		{
			GL.ClearColor(color);
			GL.Clear(ClearBufferMask.ColorBufferBit);
		}

		public void EnableBlending()
		{
			GL.Enable(EnableCap.Blend);
			GL.BlendEquation(GLEnum.FuncAdd);
			GL.BlendFuncSeparate(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha, BlendingFactor.One, BlendingFactor.Zero);
		}

		public void DisableBlending()
			=> GL.Disable(EnableCap.Blend);

		public IPipeline CreatePipeline(PipelineCompileArgs compileArgs)
		{
			try
			{
				return new OpenGLPipeline(GL, compileArgs);
			}
			finally
			{
				BindPipeline(null);
			}
		}

		public void BindPipeline(IPipeline pipeline)
		{
			_curPipeline = (OpenGLPipeline)pipeline;

			if (_curPipeline == null)
			{
				GL.BindVertexArray(0);
				GL.BindBuffer(GLEnum.ArrayBuffer, 0);
				GL.UseProgram(0);
				_curPipeline = null;
				return;
			}

			GL.BindVertexArray(_curPipeline.VAO);
			GL.BindBuffer(GLEnum.ArrayBuffer, _curPipeline.VBO);
			GL.UseProgram(_curPipeline.PID);
		}

		public void Draw(IntPtr data, int count)
		{
			if (_curPipeline == null)
			{
				throw new InvalidOperationException($"Tried to {nameof(Draw)} without pipeline!");
			}

			unsafe
			{
				var vertexes = new ReadOnlySpan<byte>((void*)data, count * _curPipeline.VertexStride);

				// BufferData reallocs and BufferSubData doesn't, so only use the former if we need to grow the buffer
				if (vertexes.Length > _curPipeline.VertexBufferLen)
				{
					GL.BufferData(GLEnum.ArrayBuffer, vertexes, GLEnum.DynamicDraw);
					_curPipeline.VertexBufferLen = vertexes.Length;
				}
				else
				{
					GL.BufferSubData(GLEnum.ArrayBuffer, 0, vertexes);
				}
			}

			GL.DrawArrays(PrimitiveType.TriangleStrip, 0, (uint)count);
		}

		public ITexture2D CreateTexture(int width, int height)
			=> new OpenGLTexture2D(GL, width, height);

		public ITexture2D WrapGLTexture2D(int glTexId, int width, int height)
			=> new OpenGLTexture2D(GL, (uint)glTexId, width, height);

		/// <exception cref="InvalidOperationException">framebuffer creation unsuccessful</exception>
		public IRenderTarget CreateRenderTarget(int width, int height)
			=> new OpenGLRenderTarget(this, GL, width, height);

		public void BindDefaultRenderTarget()
		{
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
			DefaultRenderTargetBound = true;
		}

		public Matrix4x4 CreateGuiProjectionMatrix(int width, int height)
		{
			var ret = Matrix4x4.Identity;
			ret.M11 = 2.0f / width;
			ret.M22 = 2.0f / height;
			return ret;
		}

		public Matrix4x4 CreateGuiViewMatrix(int width, int height, bool autoflip)
		{
			var ret = Matrix4x4.Identity;
			ret.M22 = -1.0f;
			ret.M41 = width * -0.5f;
			ret.M42 = height * 0.5f;
			if (autoflip && !DefaultRenderTargetBound) // flip as long as we're not a final render target
			{
				ret.M22 = 1.0f;
				ret.M42 *= -1;
			}

			return ret;
		}

		public void SetViewport(int x, int y, int width, int height)
		{
			GL.Viewport(x, y, (uint)width, (uint)height);
			GL.Scissor(x, y, (uint)width, (uint)height); // hack for mupen[rice]+intel: at least the rice plugin leaves the scissor rectangle scrambled, and we're trying to run it in the main graphics context for intel
			// BUT ALSO: new specifications.. viewport+scissor make sense together
		}
	}
}
