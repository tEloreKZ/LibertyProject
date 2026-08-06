using ProjectLiberty.Abstractions.Configuration;
using ProjectLiberty.Abstractions.Logging;
using Newtonsoft.Json.Linq;

namespace ProjectLiberty.Configuration;

/// <summary>
/// Validates configuration values against rules.
/// </summary>
public class ConfigurationValidator
{
    private readonly ILogger _logger;
    private readonly Dictionary<string, ValidationRule> _rules = new Dictionary<string, ValidationRule>();

    public ConfigurationValidator(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Adds a validation rule for a configuration key.
    /// </summary>
    /// <param name="key">Configuration key.</param>
    /// <param name="rule">Validation rule.</param>
    public void AddRule(string key, ValidationRule rule)
    {
        _rules[key] = rule;
        _logger.LogDebug($"Added validation rule for key: {key}");
    }

    /// <summary>
    /// Validates a configuration value.
    /// </summary>
    /// <param name="key">Configuration key.</param>
    /// <param name="value">Configuration value.</param>
    /// <returns>Validation result.</returns>
    public ValidationResult Validate(string key, object value)
    {
        if (!_rules.TryGetValue(key, out var rule))
        {
            return new ValidationResult { IsValid = true, Message = "No validation rule" };
        }

        try
        {
            // Type validation
            if (rule.ExpectedType != null && value != null)
            {
                if (!rule.ExpectedType.IsInstanceOfType(value))
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        Message = $"Expected type {rule.ExpectedType.Name}, got {value.GetType().Name}"
                    };
                }
            }

            // Range validation for numbers
            if (rule.Min != null && value is IComparable comparable)
            {
                if (comparable.CompareTo(rule.Min) < 0)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        Message = $"Value must be >= {rule.Min}"
                    };
                }
            }

            if (rule.Max != null && value is IComparable comparableMax)
            {
                if (comparableMax.CompareTo(rule.Max) > 0)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        Message = $"Value must be <= {rule.Max}"
                    };
                }
            }

            // String validation
            if (value is string strValue)
            {
                if (rule.MinLength != null && strValue.Length < rule.MinLength.Value)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        Message = $"String length must be >= {rule.MinLength.Value}"
                    };
                }

                if (rule.MaxLength != null && strValue.Length > rule.MaxLength.Value)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        Message = $"String length must be <= {rule.MaxLength.Value}"
                    };
                }

                if (rule.Pattern != null && !System.Text.RegularExpressions.Regex.IsMatch(strValue, rule.Pattern))
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        Message = "String does not match required pattern"
                    };
                }
            }

            // Custom validation
            if (rule.CustomValidator != null)
            {
                var customResult = rule.CustomValidator(value!);
                if (!customResult.IsValid)
                {
                    return customResult;
                }
            }

            return new ValidationResult { IsValid = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error validating configuration key: {key}");
            return new ValidationResult
            {
                IsValid = false,
                Message = $"Validation error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Validates an entire configuration object.
    /// </summary>
    /// <param name="configuration">Configuration to validate.</param>
    /// <returns>Validation result with all errors.</returns>
    public ConfigurationValidationResult ValidateConfiguration(IConfiguration configuration)
    {
        var result = new ConfigurationValidationResult();
        var keys = GetAllConfigurationKeys(configuration);

        foreach (var key in keys)
        {
            try
            {
                var value = configuration.GetValue<object>(key);
                var validation = Validate(key, value);

                if (!validation.IsValid)
                {
                    result.Errors[key] = validation.Message;
                    _logger.LogWarning($"Configuration validation failed for {key}: {validation.Message}");
                }
            }
            catch (Exception ex)
            {
                result.Errors[key] = $"Error reading value: {ex.Message}";
            }
        }

        result.IsValid = result.Errors.Count == 0;
        return result;
    }

    private IEnumerable<string> GetAllConfigurationKeys(IConfiguration configuration)
    {
        // This would need to be implemented based on actual configuration structure
        // For now, return the keys that have validation rules
        return _rules.Keys;
    }
}

/// <summary>
/// Validation rule for configuration values.
/// </summary>
public class ValidationRule
{
    public Type? ExpectedType { get; set; }
    public IComparable? Min { get; set; }
    public IComparable? Max { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public string? Pattern { get; set; }
    public Func<object?, ValidationResult>? CustomValidator { get; set; }
}

/// <summary>
/// Validation result for a single value.
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Validation result for an entire configuration.
/// </summary>
public class ConfigurationValidationResult
{
    public bool IsValid { get; set; }
    public Dictionary<string, string> Errors { get; set; } = new Dictionary<string, string>();
}
