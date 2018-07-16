#ifndef _BOUNDS_CHECKED_ARRAY_H_
#define _BOUNDS_CHECKED_ARRAY_H_

#include <array>
#include <functional>
#include "util.h"

namespace hel{
    /**
     * \struct BoundsCheckedArray
     * \brief Array type with bounds checking and helpful operators
     */
    template<typename T, std::size_t LEN>
    struct BoundsCheckedArray{
    private:
        std::array<T, LEN> internal;

    public:
        constexpr const T& at(std::size_t pos)const{
            if(pos < 0 || pos >= LEN){
                throw std::out_of_range("Exception: array index out of bounds: index " + std::to_string(pos) + " in array of size " + std::to_string(LEN));
            }
            return internal[pos];
        }

        constexpr T& at(std::size_t pos){
            if(pos < 0 || pos >= LEN){
                throw std::out_of_range("Exception: array index out of bounds: index " + std::to_string(pos) + " in array of size " + std::to_string(LEN));
            }
            return internal[pos];
        }

        constexpr const T& operator[](std::size_t pos)const{
            if(pos < 0 || pos >= LEN){
                throw std::out_of_range("Exception: array index out of bounds: index " + std::to_string(pos) + " in array of size " + std::to_string(LEN));
            }
            return internal[pos];
        }

        constexpr T& operator[](std::size_t pos){
            if(pos < 0 || pos >= LEN){
                throw std::out_of_range("Exception: array index out of bounds: index " + std::to_string(pos) + " in array of size " + std::to_string(LEN));
            }
            return internal[pos];
        }

        constexpr const T& front()const{
            if(LEN == 0){
                throw std::out_of_range("Exception: array index out of bounds: cannot refernece front of array of size " + std::to_string(LEN));
            }
            return internal.front();
        }

        constexpr T& front(){
            if(LEN == 0){
                throw std::out_of_range("Exception: array index out of bounds: cannot refernece front of array of size " + std::to_string(LEN));
            }
            return internal.front();
        }

        constexpr const T& back()const{
            if(LEN == 0){
                throw std::out_of_range("Exception: array index out of bounds: cannot refernece back of array of size " + std::to_string(LEN));
            }
            return internal.back();
        }

        constexpr T& back(){
            if(LEN == 0){
                throw std::out_of_range("Exception: array index out of bounds: cannot refernece back of array of size " + std::to_string(LEN));
            }
            return internal.back();
        }

        constexpr const T* data()const{
            if(LEN == 0){
                return nullptr;
            }
            return internal.data();
        }

        constexpr T* data(){
            if(LEN == 0){
                return nullptr;
            }
            return internal.data();
        }

        constexpr std::size_t size()const{
            return internal.size();
        }

        constexpr bool empty()const{
            return internal.empty();
        }

        constexpr std::size_t max_size()const{
            return internal.max_size();
        }

        constexpr typename std::array<T, LEN>::iterator begin(){
            return internal.begin();
        }

        constexpr const typename std::array<T, LEN>::iterator begin()const{
            return internal.begin();
        }

        constexpr typename std::array<T, LEN>::iterator end(){
            return internal.end();
        }

        constexpr const typename std::array<T, LEN>::iterator end()const{
            return internal.end();
        }

        BoundsCheckedArray& operator=(std::array<T, LEN> const& a){
            internal = a;
            return *this;
        }

        BoundsCheckedArray(){}

        BoundsCheckedArray(std::initializer_list<T> list){
            if(list.size() != LEN){
                throw std::out_of_range("Exception: assignement to array of size " + std::to_string(LEN) + " to brace-enclosed initializer list of different size " + std::to_string(list.size()));
            }
            std::copy(list.begin(), list.end(), internal.begin());
        }

        template<typename S>
        BoundsCheckedArray(S iterable){
            if(iterable.size() != LEN){
                throw std::out_of_range("Exception: assignement to array of size " + std::to_string(LEN) + " to iterable of different size " + std::to_string(iterable.size()));
            }
            std::copy(iterable.begin(), iterable.end(), internal.begin());
        }

        bool operator==(const BoundsCheckedArray<T, LEN>& b){
            return internal == b.internal;
        }

        bool operator!=(const BoundsCheckedArray<T, LEN>& b){
            return !(*this == b);
        }

        template<typename S, std::size_t L>
        friend std::string to_string(const BoundsCheckedArray<S, L>&, std::function<std::string(S)>);
    };

    template<typename T, std::size_t LEN>
    std::string to_string(const BoundsCheckedArray<T, LEN>& a, std::function<std::string(T)> to_s){
        return hel::to_string(a.internal, to_s);
    }
}

#endif
