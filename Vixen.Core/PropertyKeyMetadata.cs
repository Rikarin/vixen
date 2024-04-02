namespace Vixen.Core;

/// <summary>
///     Specifies metadata for an <see cref="PropertyKey" />.
/// </summary>
public abstract class PropertyKeyMetadata { }

public abstract class DefaultValueMetadata<T> : DefaultValueMetadata
    {
        /// <summary>
        /// Gets the default value of an external property, and specify if this default value should be kept.
        /// It could be usefull with properties with default values depending of its container, especially if they are long to generate.
        /// An example would be collision data, which should be generated only once.
        /// </summary>
        /// <param name="obj">The property container.</param>
        /// <returns>The default value.</returns>
        public abstract T GetDefaultValueT(ref PropertyContainer obj);

        public override object GetDefaultValue(ref PropertyContainer obj)
        {
            return GetDefaultValueT(ref obj);
        }
    }

    /// <summary>
    /// Defines default value of a specific <see cref="PropertyKey"/> as a parameter value.
    /// </summary>
    public class StaticDefaultValueMetadata<T> : DefaultValueMetadata<T>
    {
        private readonly T defaultValue;
        private readonly bool keepDefaultValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticDefaultValueMetadata{T}"/> class.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="keepDefaultValue">if set to <c>true</c> [keep default value].</param>
        public StaticDefaultValueMetadata(T defaultValue, bool keepDefaultValue = false)
        {
            this.defaultValue = defaultValue;
            this.keepDefaultValue = keepDefaultValue;
        }

        /// <inheritdoc/>
        public override T GetDefaultValueT(ref PropertyContainer obj)
        {
            return defaultValue;
        }

        /// <inheritdoc/>
        public override bool KeepValue
        {
            get
            {
                return keepDefaultValue;
            }
        }
    }

    /// <summary>
    /// Specifies a delegate to fetch the default value of an <see cref="PropertyKey"/>.
    /// </summary>
    public class DelegateDefaultValueMetadata<T> : DefaultValueMetadata<T>
    {
        private readonly DefaultValueCallback callback;

        /// <summary>
        /// Callback used to initialiwe the tag value.
        /// </summary>
        /// <param name="container">The tag property container.</param>
        /// <returns>Value of the tag.</returns>
        public delegate T DefaultValueCallback(ref PropertyContainer container);

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateDefaultValueMetadata{T}"/> class.
        /// </summary>
        /// <param name="callback">The callback.</param>
        public DelegateDefaultValueMetadata(DefaultValueCallback callback)
        {
            this.callback = callback;
        }

        public override T GetDefaultValueT(ref PropertyContainer obj)
        {
            return callback(ref obj);
        }

        public override bool KeepValue
        {
            get { return true; }
        }
    }
