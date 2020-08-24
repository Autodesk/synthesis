#ifndef CONTROLLER_SYS_H
#define CONTROLLER_SYS_H

enum LogLevel {
    LOG_LEVEL_INFO = 0,
    LOG_LEVEL_DEBUG = 1,
    LOG_LEVEL_WARNING = 2,
    LOG_LEVEL_ERROR = 3
};

void log_str(const char* message, int log_level, int* error_code, const char** error_message, const char** error_data);
void forward(unsigned channel, double distance, int* error_code, const char** error_message, const char** error_data);
void backward(unsigned channel, double distance, int* error_code, const char** error_message, const char** error_data);
void left(unsigned channel, double distance, int* error_code, const char** error_message, const char** error_data);
void right(unsigned channel, double distance, int* error_code, const char** error_message, const char** error_data);
void up(unsigned channel, double distance, int* error_code, const char** error_message, const char** error_data);
void down(unsigned channel, double distance, int* error_code, const char** error_message, const char** error_data);
int test(int value, int* error_code, const char** error_message, const char** error_data);
int set_motor_percent(unsigned channel, int motor_index, float percent, int* error_code, const char** error_message, const char** error_data);

#endif